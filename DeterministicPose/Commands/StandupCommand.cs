using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using DeterministicPose.Chat;

namespace DeterministicPose.Commands;

public class StandupCommand(ChatSender chatSender, ICommandManager commandManager, ITargetManager targetManager) : BaseCommand(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/standup";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME}";

    private static readonly string TARGET_FORWARD_COMMAND = "/action \"target forward\"";

    private ChatSender ChatSender { get; init; } = chatSender;
    private ITargetManager TargetManager { get; init; } = targetManager;

    protected override void Handler(string command, string args)
    {
        var savedTarget = TargetManager.Target;
        TargetManager.Target = null;
        ChatSender.SendMessage(TARGET_FORWARD_COMMAND);
        TargetManager.Target = savedTarget;
    }
}
