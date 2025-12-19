using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using DeterministicPose.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Globalization;
using System.Numerics;

namespace DeterministicPose.Cmds;

public unsafe class IfProximityCmd(ICommandManager commandManager, IChatGui chatGui, ChatSender chatSender, IObjectTable objectTable, ITargetManager targetManager, IPluginLog pluginLog) : BaseResolveCmd(commandManager, COMMAND_NAME, COMMAND_HELP_MESSAGE, objectTable, targetManager)
{
    private static readonly string COMMAND_NAME = "/ifproximity";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} <player name>( <range>)?";

    private static readonly string ABORT_COMMAND = "/macrocancel";

    private IChatGui ChatGui { get; init; } = chatGui;
    private ChatSender ChatSender { get; init; } = chatSender;
    private IPluginLog PluginLog { get; init; } = pluginLog;

    protected override void Handler(string command, string args)
    {
        var parsedArgs = Arguments.SplitCommandLine(args);
        if (parsedArgs.Length > 2)
        {
            ChatGui.PrintError(COMMAND_HELP_MESSAGE);
            return;
        }

        var localPlayer = ObjectTable.LocalPlayer;
        if (localPlayer == null)
        {
            ChatGui.PrintError("No local player");
            return;
        }

        var localCharacter = (Character*)localPlayer.Address;
        if (localCharacter->DrawObject == null)
        {
            ChatGui.PrintError("No draw object for local player");
            return;
        }

        var name = parsedArgs[0];
        var player = FindPlayerCharacter(name);
        if (player == null)
        {
            ChatGui.PrintError($"Sending {ABORT_COMMAND} since unable to locate: {name}");
            ChatSender.SendMessage(ABORT_COMMAND);
            return;
        }

        var character = (Character*)player.Address;
        if (character->DrawObject == null)
        {
            ChatGui.PrintError($"No draw object for character: {name}");
            return;
        }

        var proximityRange = parsedArgs.Length == 2 && float.TryParse(parsedArgs[1], CultureInfo.InvariantCulture, out var range) ? range : 1;

        var distance = Vector3.Distance(localCharacter->DrawObject->Position, character->DrawObject->Position);
        PluginLog.Debug($"Player '{player.Name}' found at distance: {distance}");
        if (distance > proximityRange)
        {
            PluginLog.Debug($"Player '{player.Name}' is not in proximity (range {proximityRange})");
            ChatSender.SendMessage(ABORT_COMMAND);
        } 
        else
        {
            PluginLog.Debug($"Player '{player.Name}' is in proximity (range {proximityRange})");
            
        }
    }
}
