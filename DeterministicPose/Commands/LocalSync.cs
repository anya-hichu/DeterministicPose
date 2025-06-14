using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using DeterministicPose.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Component.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeterministicPose.Commands;

// Reference: https://github.com/Caraxi/SimpleHeels/blob/0a0fe3c02a0a2c5a7c96b3304952d5078cd338aa/Plugin.cs#L638

public unsafe class LocalSync(IChatGui chatGui, IClientState clientState, ICommandManager commandManager, INotificationManager notificationManager, IObjectTable objectTable, ITargetManager targetManager) : BaseCommand(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/localsync";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} <from>( <to>)?";

    private static readonly int MAX_RETRIES = 40;
    private static readonly int RETRY_WAIT_MS = 25;

    private IChatGui ChatGui { get; init; } = chatGui;
    private IClientState ClientState { get; init; } = clientState;
    private INotificationManager NotificationManager { get; init; } = notificationManager;
    private IObjectTable ObjectTable { get; init; } = objectTable;
    private ITargetManager TargetManager { get; init; } = targetManager;
    

    private CancellationTokenSource? CancellationTokenSource { get; set; }

    protected override void Handler(string command, string args)
    {
        var parsedArgs = Arguments.SplitCommandLine(args);

        var numParsedArgs = parsedArgs.Length;
        if (numParsedArgs < 1 || numParsedArgs > 2)
        {
            ChatGui.PrintError(COMMAND_HELP_MESSAGE);
            return;
        }

        var sourceName = parsedArgs[0];
        var sourcePlayer = FindPlayerCharacter(sourceName);

        if (sourcePlayer == null)
        {
            ChatGui.PrintError($"Failed to locate source '{sourceName}'");
            return;
        }

        var targetName = numParsedArgs > 1 ? parsedArgs[1] : "<me>";
        var targetPlayer = FindPlayerCharacter(targetName);

        if (targetPlayer == null)
        {
            ChatGui.PrintError($"Failed to locate target '{targetName}'");
            return;
        }

        var token = HandleCancellation();
        Task.Run(() =>
        {
            var success = false;
            for (var retries = 0; !success && retries < MAX_RETRIES && !token.IsCancellationRequested; retries++)
            {
                if (TryGetAnimationLocalTime(sourcePlayer, out var localTime))
                {
                    if (TrySetAnimationLocalTime(targetPlayer, localTime!.Value))
                    {
                        success = true;
                        NotificationManager.AddNotification(new()
                        {
                            Title = $"Success {sourcePlayer.Name} -> {targetPlayer.Name} (#{Task.CurrentId})",
                            Type = NotificationType.Success,
                            Content = $"Copied animation local time ({localTime}) from '{sourcePlayer.Name}' to '{targetPlayer.Name}'",
                            Minimized = false
                        });
                    }
                }
                Thread.Sleep(RETRY_WAIT_MS);
            }

            if (token.IsCancellationRequested)
            {
                NotificationManager.AddNotification(new()
                {
                    Title = $"Cancel {sourcePlayer.Name} -> {targetPlayer.Name} (#{Task.CurrentId})",
                    Type = NotificationType.Error,
                    Content = $"Cancelled animation local time copy from '{sourcePlayer.Name}' to '{targetPlayer.Name}'",
                    Minimized = false
                });
                return;
            }

            if (!success)
            {
                NotificationManager.AddNotification(new()
                {
                    Title = $"Failed {sourcePlayer.Name} -> {targetPlayer.Name} (#{Task.CurrentId})",
                    Type = NotificationType.Error,
                    Content = $"Failed to copy animation local time from '{sourcePlayer.Name}' to '{targetPlayer.Name}' after {MAX_RETRIES} tries (waited {MAX_RETRIES * RETRY_WAIT_MS} ms)",
                    Minimized = false
                });
                return;
            }
        }, token);
    }

    private CancellationToken HandleCancellation()
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource = new();
        return CancellationTokenSource.Token;
    }

    private bool TryGetAnimationLocalTime(IPlayerCharacter player, out float? localTime) => TryAccessAnimationLocalTime(player, null, out localTime);
    private bool TrySetAnimationLocalTime(IPlayerCharacter player, float newLocalTime) => TryAccessAnimationLocalTime(player, newLocalTime, out var _);
    private bool TryAccessAnimationLocalTime(IPlayerCharacter player, float? newLocalTime, out float? localTime)
    {
        localTime = null;

        var character = (Character*)player.Address;
        if (character->DrawObject == null) return false;
        if (character->DrawObject->GetObjectType() != ObjectType.CharacterBase) return false;
        if (((CharacterBase*)character->DrawObject)->GetModelType() != CharacterBase.ModelType.Human) return false;
        var human = (Human*)character->DrawObject;
        if (character->Mode is CharacterModes.Mounted or CharacterModes.RidingPillion) return false;
        if (character->Mode is not (CharacterModes.InPositionLoop or CharacterModes.EmoteLoop)) return false;
        var skeleton = human->Skeleton;

        for (var i = 0; i < skeleton->PartialSkeletonCount && i < 1; i++)
        {
            var partialSkeleton = &skeleton->PartialSkeletons[i];
            var animatedSkeleton = partialSkeleton->GetHavokAnimatedSkeleton(0);
            if (animatedSkeleton == null) continue;
            for (var j = 0; j < animatedSkeleton->AnimationControls.Length && j < 1; j++)
            {
                var control = animatedSkeleton->AnimationControls[j].Value;
                if (control == null) continue;

                localTime = control->hkaAnimationControl.LocalTime;
                if (newLocalTime.HasValue)
                {
                    control->hkaAnimationControl.LocalTime = newLocalTime.Value;
                }
            }
        }
        return localTime != null;
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
