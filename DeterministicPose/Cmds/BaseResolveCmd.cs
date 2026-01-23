using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
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
            _ => ObjectTable.FirstOrDefault(obj =>
            {
                if (obj is not IPlayerCharacter playerCharacter) return false;

                var playerName = obj.Name.ToString();
                if (playerName == name) return true;

                var playerFullName = $"{playerName}@{playerCharacter.HomeWorld.Value.Name}";
                
                return playerFullName == name;
            })!,
        };

        return gameObject is IPlayerCharacter playerCharacter ? playerCharacter : null;
    }
}
