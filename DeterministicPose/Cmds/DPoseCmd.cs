using Dalamud.Plugin.Services;
using Dalamud.Utility;
using DeterministicPose.Managers;

namespace DeterministicPose.Cmds;

public class DPoseCmd(IChatGui chatGui, ICommandManager commandManager, CPoseManager cPoseManager) : BaseCmd(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/dpose";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} <index>";

    private CPoseManager CPoseManager { get; init; } = cPoseManager;
    private IChatGui ChatGui { get; init; } = chatGui;

    protected override void Handler(string command, string args)
    {
        if (args.IsNullOrWhitespace())
        {
            ChatGui.Print($"Current pose index: {CPoseManager.GetCurrentPoseIndex()}");
        }
        else
        {
            if (byte.TryParse(args, out var index))
            {
                CPoseManager.Change(index);
            }
            else
            {
                ChatGui.Print(COMMAND_HELP_MESSAGE);
            }
        }
    }
}
