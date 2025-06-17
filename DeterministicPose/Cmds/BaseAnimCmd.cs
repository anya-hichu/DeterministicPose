using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace DeterministicPose.Cmds;

// Reference: https://github.com/Caraxi/SimpleHeels/blob/0a0fe3c02a0a2c5a7c96b3304952d5078cd338aa/Plugin.cs#L638

public abstract unsafe class BaseAnimCmd(IClientState clientState, ICommandManager commandManager, string command, string commandHelpMessage, IObjectTable objectTable, ITargetManager targetManager) : BaseResolveCmd(clientState, commandManager, command, commandHelpMessage, objectTable, targetManager)
{
    protected bool TryGetAnimationLocalTime(IPlayerCharacter player, out float? localTime) => TryAccessAnimationLocalTime(player, null, out localTime);
    protected bool TrySetAnimationLocalTime(IPlayerCharacter player, float newLocalTime) => TryAccessAnimationLocalTime(player, newLocalTime, out var _);

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
}
