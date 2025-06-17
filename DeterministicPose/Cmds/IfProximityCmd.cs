using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using DeterministicPose.Utils;

namespace DeterministicPose.Cmds;

public class IfProximityCmd(ICommandManager commandManager, IClientState clientState, IChatGui chatGui, ChatSender chatSender, IObjectTable objectTable, ITargetManager targetManager, IPluginLog pluginLog) : BaseResolveCmd(clientState, commandManager, COMMAND_NAME, COMMAND_HELP_MESSAGE, objectTable, targetManager)
{
    private static readonly string COMMAND_NAME = "/ifproximity";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} <player name>";

    private static readonly string ABORT_COMMAND = "/macrocancel";

    private IChatGui ChatGui { get; init; } = chatGui;
    private ChatSender ChatSender { get; init; } = chatSender;
    private IPluginLog PluginLog { get; init; } = pluginLog;

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


}
