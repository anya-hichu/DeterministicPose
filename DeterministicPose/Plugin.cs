using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using DeterministicPose.Commands;
using DeterministicPose.Managers;

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
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;

    private DPoseCommand DPoseCommand { get; init; }
    private StandupCommand StandupCommand { get; init; }
    private IfInThatPositionCommand IfInThatPositionCommand { get; init; }
    private UntargetCommand UntargetCommand { get; init; }
    private IfProximity IfProximity { get; init; } 
    private LocalSync LocalSyncCommand { get; init; }

    public Plugin()
    {
        var chatSender = new ChatSender(new(SigScanner), PluginLog);
        var cPoseManager = new CPoseManager(ChatGui, ClientState, chatSender);
        DPoseCommand = new(ChatGui, CommandManager, cPoseManager);
        StandupCommand = new(chatSender, CommandManager, TargetManager);
        IfInThatPositionCommand = new(ChatGui, chatSender, Condition, CommandManager);
        UntargetCommand = new(TargetManager, CommandManager);
        IfProximity = new(CommandManager, ClientState, ChatGui, chatSender, ObjectTable, TargetManager, PluginLog);
        LocalSyncCommand = new(ChatGui, ClientState, CommandManager, NotificationManager, ObjectTable, TargetManager);
    }

    public void Dispose()
    {
        DPoseCommand.Dispose();
        StandupCommand.Dispose();
        IfInThatPositionCommand.Dispose();
        UntargetCommand.Dispose();
        IfProximity.Dispose();
        LocalSyncCommand.Dispose();
    }
}
