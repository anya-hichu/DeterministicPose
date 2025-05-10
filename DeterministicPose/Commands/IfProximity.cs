using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using DeterministicPose.Chat;
using DeterministicPose.Utils;
using System.Runtime.Serialization;
using System.Linq;

namespace DeterministicPose.Commands;

class IfProximity : BaseCommand
{
    private static readonly string COMMAND_NAME = "/ifproximity";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: /ifproximity <player name>";

    private static readonly string ABORT_COMMAND = "/macrocancel";

    private IClientState ClientState { get; init; }
    private IChatGui ChatGui { get; init; }
    private ChatSender ChatSender { get; init; }
    private IObjectTable ObjectTable { get; init; }
    private ITargetManager TargetManager { get; init; }
    private IPluginLog PluginLog { get; init; }

    public IfProximity(ICommandManager commandManager, IClientState clientState, IChatGui chatGui, ChatSender chatSender, IObjectTable objectTable, ITargetManager targetManager, IPluginLog pluginLog) : base(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
    {
        ClientState = clientState;
        ChatGui = chatGui;
        ChatSender = chatSender;
        ObjectTable = objectTable;
        TargetManager = targetManager;
        PluginLog = pluginLog;
    }

    protected override void Handler(string command, string args)
    {
        var parsedArgs = Arguments.SplitCommandLine(args);
        if (parsedArgs.Length == 1)
        {
            var name = parsedArgs[0];
            var player = FindPlayerCharacter(name);
            if (player != null)
            {
                PluginLog.Debug($"Player '{player.Name}' found at distance X: {player.YalmDistanceX}, Z: {player.YalmDistanceZ} (yalms)");
                if (player.YalmDistanceX != 0 || player.YalmDistanceZ != 0)
                {
                    ChatSender.SendMessage(ABORT_COMMAND);
                } 
            }
            else
            {
                ChatGui.PrintError($"Sending {ABORT_COMMAND} since unable to locate: {name}");
                ChatSender.SendMessage(ABORT_COMMAND);
            }
        } 
        else
        {
            ChatGui.PrintError(COMMAND_HELP_MESSAGE);
        }
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
