using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using DeterministicPose.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using System.Linq;
using System.Threading;

namespace DeterministicPose.Commands;

// Reference: https://github.com/Caraxi/SimpleHeels/blob/0a0fe3c02a0a2c5a7c96b3304952d5078cd338aa/Plugin.cs#L638

public unsafe class LocalSync(IChatGui chatGui, IClientState clientState, ICommandManager commandManager, IFramework framework, IObjectTable objectTable, ITargetManager targetManager) : BaseCommand(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/localsync";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} <player name>";

    private static readonly int MAX_RETRIES = 5;
    private static readonly int RETRY_WAIT_MS = 30;

    private IChatGui ChatGui { get; init; } = chatGui;
    private IClientState ClientState { get; init; } = clientState;
    private IFramework Framework { get; init; } = framework;
    private IObjectTable ObjectTable { get; init; } = objectTable;
    private ITargetManager TargetManager { get; init; } = targetManager;

    protected override void Handler(string command, string args)
    {
        var localPlayer = ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }

        var parsedArgs = Arguments.SplitCommandLine(args);
        if (parsedArgs.Length == 1)
        {
            var name = parsedArgs[0];
            var player = FindPlayerCharacter(name);
            if (player == null)
            {
                ChatGui.PrintError($"Failed to locate player: {name}");
                return;
            }

            Framework.RunOnFrameworkThread(() =>
            {
                var success = false;
                for (var retries = 0; !success && retries < MAX_RETRIES; retries++)
                {
                    if (TryGetAnimationLocalTime(player, out var localTime))
                    {
                        if (TrySetAnimationLocalTime(localPlayer, localTime))
                        {
                            success = true;
                        }
                    }
                    Thread.Sleep(RETRY_WAIT_MS);
                }

                if (!success)
                {
                    ChatGui.PrintError($"Failed to set sync animation local time with '{player.Name}' after {MAX_RETRIES} tries (waited {RETRY_WAIT_MS * MAX_RETRIES}ms)");
                }
            });
        } 
        else
        {
            ChatGui.PrintError(COMMAND_HELP_MESSAGE);
        }
            
    }

    private bool TryGetAnimationLocalTime(IPlayerCharacter player, out float localTime) => TryGetAndSetAnimationLocalTime(player, null, out localTime);
    private bool TrySetAnimationLocalTime(IPlayerCharacter player, float newLocalTime) => TryGetAndSetAnimationLocalTime(player, newLocalTime, out var _);
    private bool TryGetAndSetAnimationLocalTime(IPlayerCharacter player, float? newLocalTime, out float localTime)
    {
        localTime = default;

        var character = (Character*)player.Address;
        if (character->DrawObject == null) return false;
        if (character->DrawObject->GetObjectType() != ObjectType.CharacterBase) return false;
        if (((CharacterBase*)character->DrawObject)->GetModelType() != CharacterBase.ModelType.Human) return false;
        var human = (Human*)character->DrawObject;
        if (character->Mode is CharacterModes.Mounted or CharacterModes.RidingPillion) return false;
        if (character->Mode is not (CharacterModes.InPositionLoop or CharacterModes.EmoteLoop)) return false;
        var skeleton = human->Skeleton;

        for (var i = 0; i < skeleton->PartialSkeletonCount && i < 1; ++i)
        {
            var partialSkeleton = &skeleton->PartialSkeletons[i];
            var animatedSkeleton = partialSkeleton->GetHavokAnimatedSkeleton(0);
            if (animatedSkeleton == null) continue;
            for (var j = 0; j < animatedSkeleton->AnimationControls.Length && j < 1; ++j)
            {
                var control = animatedSkeleton->AnimationControls[j].Value;
                if (control == null) continue;

                localTime = control->hkaAnimationControl.LocalTime;
                if (newLocalTime.HasValue)
                {
                    control->hkaAnimationControl.LocalTime = newLocalTime.Value;
                }
                return true;
            }
        }
        return false;
    }

    private IPlayerCharacter? FindPlayerCharacter(string name)
    {
        var gameObject = name switch
        {
            _ when name.IsNullOrWhitespace() => null,
            "<me>" or "self" => ClientState.LocalPlayer,
            "<t>" or "target" => ClientState.LocalPlayer?.TargetObject,
            "<f>" or "focus" => TargetManager.FocusTarget,
            "<mo>" or "mouseover" => TargetManager.MouseOverTarget,
            _ => ObjectTable.FirstOrDefault(o => o.Name.TextValue == name)!,
        };

        return gameObject is IPlayerCharacter playerCharacter ? playerCharacter : null;
    }
}
