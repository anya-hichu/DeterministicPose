using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;

namespace DeterministicPose.Commands;

public unsafe class UntargetCommand(ITargetManager targetManager, ICommandManager commandManager) : BaseCommand(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/untarget";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME}";

    private ITargetManager TargetManager { get; init; } = targetManager;

    protected override void Handler(string command, string args)
    {
        TargetManager.Target = null;
    }
}
