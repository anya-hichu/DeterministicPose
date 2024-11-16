using Dalamud.Plugin.Services;
using DeterministicPose.Managers;

namespace DeterministicPose.Commands;

public class DPoseCommand(IChatGui chatGui, ICommandManager commandManager, CPoseManager cPoseManager) : BaseCommand(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/dpose";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} [Index]";

    private CPoseManager CPoseManager { get; init; } = cPoseManager;
    private IChatGui ChatGui { get; init; } = chatGui;

    protected override void Handler(string command, string args)
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
}
