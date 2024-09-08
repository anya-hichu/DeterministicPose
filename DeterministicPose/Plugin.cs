using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace DeterministicPose;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

    private const string CommandName = "/dpose";
    private const string CommandHelpMessage = $"Command usage: {CommandName} <index>";

    private CPoseManager CPoseManager { get; init; }

    public Plugin()
    {
        CPoseManager = new(ClientState, new(SigScanner), PluginLog);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = CommandHelpMessage
        });

        PluginInterface.UiBuilder.OpenConfigUi += Noop;
        PluginInterface.UiBuilder.OpenMainUi += Noop;
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        if (byte.TryParse(args, out var index))
        {
            CPoseManager.Change(index);
        } 
        else
        {
            ChatGui.Print(CommandHelpMessage);
        }
    }

    private void Noop() { }
}
