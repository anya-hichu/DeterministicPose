using Dalamud.Plugin.Services;
using DeterministicPose.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace DeterministicPose.Cmds;

public unsafe class WalkCmd(IChatGui chatGui, ICommandManager commandManager) : BaseCmd(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/walk";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} (enable|disable|toggle)?";

    private IChatGui ChatGui { get; init; } = chatGui;

    protected override void Handler(string command, string args)
    {
        var parsedArgs = Arguments.SplitCommandLine(args);

        if (parsedArgs.Length > 1)
        {
            ChatGui.PrintError(COMMAND_HELP_MESSAGE);
            return;
        }

        var controlInstance = Control.Instance();

        // on and off for backward compatibility
        if (parsedArgs.Length == 0 || parsedArgs[0] == "enable" || parsedArgs[0] == "on")
        {
            controlInstance->IsWalking = true;
            return;
        }

        if (parsedArgs[0] == "disable" || parsedArgs[0] == "off")
        {
            controlInstance->IsWalking = false;
            return;
        }

        if (parsedArgs[0] == "toggle")
        {
            controlInstance->IsWalking = !controlInstance->IsWalking;
            return;
        }

        ChatGui.PrintError(COMMAND_HELP_MESSAGE);
    }
}
