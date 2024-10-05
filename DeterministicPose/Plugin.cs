using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using DeterministicPose.Commands;

namespace DeterministicPose;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;

    private DPoseCommand DPoseCommand { get; init; }
    private StandupCommand StandupCommand { get; init; }
    private IfInThatPositionCommand IfInThatPositionCommand { get; init; }

    public Plugin()
    {
        var chatSender = new ChatSender(new(SigScanner), PluginLog);
        DPoseCommand = new(ChatGui, CommandManager, new(ClientState, chatSender, PluginLog));
        StandupCommand = new(chatSender, CommandManager);
        IfInThatPositionCommand = new(ChatGui, chatSender, Condition, CommandManager);

        PluginInterface.UiBuilder.OpenConfigUi += Noop;
        PluginInterface.UiBuilder.OpenMainUi += Noop;
    }

    public void Dispose()
    {
        DPoseCommand.Dispose();
        StandupCommand.Dispose();
        IfInThatPositionCommand.Dispose();
    }

    private void Noop() { }
}
