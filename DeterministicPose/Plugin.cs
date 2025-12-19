using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using DeterministicPose.Cmds;
using DeterministicPose.Managers;
using Lumina.Excel.Sheets;

namespace DeterministicPose;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;


    private DPoseCmd DPoseCmd { get; init; }
    private StandupCmd StandupCmd { get; init; }
    private IfInThatPositionCmd IfInThatPositionCmd { get; init; }
    private UntargetCmd UntargetCmd { get; init; }
    private IfProximityCmd IfProximityCmd { get; init; } 
    private LocalSyncCmd LocalSyncCmd { get; init; }
    private RemoteSyncCmd RemoteSyncCmd { get; init; }
    private WalkCmd WalkCmd { get; init; }

    public Plugin()
    {
        var chatSender = new ChatSender(new(SigScanner), PluginLog);
        var cPoseManager = new CPoseManager(ChatGui, chatSender, ObjectTable);

        DPoseCmd = new(ChatGui, CommandManager, cPoseManager);
        StandupCmd = new(chatSender, CommandManager, TargetManager);
        IfInThatPositionCmd = new(ChatGui, chatSender, Condition, CommandManager);
        UntargetCmd = new(TargetManager, CommandManager);
        IfProximityCmd = new(CommandManager, ChatGui, chatSender, ObjectTable, TargetManager, PluginLog);
        LocalSyncCmd = new(ChatGui, CommandManager, ObjectTable, PluginLog, TargetManager);

        var emoteSheet = DataManager.GetExcelSheet<Emote>()!;
        RemoteSyncCmd = new(ChatGui, chatSender, cPoseManager, CommandManager, emoteSheet, ObjectTable, PluginLog, TargetManager);
        WalkCmd = new(ChatGui, CommandManager);
    }

    public void Dispose()
    {
        DPoseCmd.Dispose();
        StandupCmd.Dispose();
        IfInThatPositionCmd.Dispose();
        UntargetCmd.Dispose();
        IfProximityCmd.Dispose();
        LocalSyncCmd.Dispose();
        RemoteSyncCmd.Dispose();
        WalkCmd.Dispose();
    }
}
