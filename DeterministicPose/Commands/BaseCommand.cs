using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using System;

namespace DeterministicPose.Commands;

public abstract class BaseCommand : IDisposable
{
    protected string Command { init; get; }
    protected string CommandHelpMessage { get; init; }
    protected ICommandManager CommandManager { get; init; }

    public BaseCommand(string command, string commandHelpMessage, ICommandManager commandManager)
    {
        Command = command;
        CommandHelpMessage = commandHelpMessage;
        CommandManager = commandManager;

        CommandManager.AddHandler(command, new CommandInfo(Handler)
        {
            HelpMessage = commandHelpMessage
        });
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler(Command);
    }

    protected abstract void Handler(string command, string args);
}
