using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using System.Linq;

namespace DeterministicPose.Cmds;

public abstract class BaseResolveCmd(IClientState clientState, ICommandManager commandManager, string command, string commandHelpMessage, IObjectTable objectTable, ITargetManager targetManager) : BaseCmd(command, commandHelpMessage, commandManager)
{
    protected IClientState ClientState { get; init; } = clientState;
    protected IObjectTable ObjectTable { get; init; } = objectTable;
    protected ITargetManager TargetManager { get; init; } = targetManager;

    protected IPlayerCharacter? FindPlayerCharacter(string name)
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
