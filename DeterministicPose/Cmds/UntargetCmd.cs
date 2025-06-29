using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;

namespace DeterministicPose.Cmds;

public unsafe class UntargetCmd(ITargetManager targetManager, ICommandManager commandManager) : BaseCmd(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    public static readonly string COMMAND_NAME = "/untarget";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME}";

    protected override void Handler(string command, string args)
    {
        targetManager.Target = null;
    }
}
