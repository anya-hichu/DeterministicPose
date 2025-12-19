using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Linq;

namespace DeterministicPose.Cmds;

public abstract unsafe class BaseResolveCmd(ICommandManager commandManager, string command, string commandHelpMessage, IObjectTable objectTable, ITargetManager targetManager) : BaseCmd(command, commandHelpMessage, commandManager)
{
    protected IObjectTable ObjectTable { get; init; } = objectTable;
    protected ITargetManager TargetManager { get; init; } = targetManager;

    protected IPlayerCharacter? FindPlayerCharacter(string name)
    {
        var gameObject = name switch
        {
            _ when name.IsNullOrWhitespace() => null,
            "<me>" or "self" => ObjectTable.LocalPlayer,
            "<t>" or "target" => ObjectTable.LocalPlayer?.TargetObject,
            "<f>" or "focus" => TargetManager.FocusTarget,
            "<mo>" or "mouseover" => TargetManager.MouseOverTarget,
            _ => ObjectTable.FirstOrDefault(o => o.ObjectKind == ObjectKind.Player && ((Character*)o.Address)->NameString == name)!,
        };

        return gameObject is IPlayerCharacter playerCharacter ? playerCharacter : null;
    }
}
