using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using DeterministicPose.Utils;
using System.Collections.Generic;
using System.Linq;

namespace DeterministicPose.Commands;

public class IfInThatPositionCommand(IChatGui chatGui, ChatSender chatSender, ICondition condition, ICommandManager commandManager) : BaseCommand(COMMAND_NAME, COMMAND_HELP_MESSAGE, commandManager)
{
    private static readonly string COMMAND_NAME = "/ifinthatposition";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} -(?|!|$|v)( [command])?";

    private static readonly char VERBOSE_FLAG = '?';
    private static readonly char DRY_RUN_FLAG = '!';
    private static readonly char ABORT_ON_FALSE_FLAG = '@';
    private static readonly char ABORT_ON_TRUE_FLAG = '$';
    private static readonly char INVERSE_FLAG = 'v';
    private static readonly HashSet<char> KNOWN_FLAGS = [DRY_RUN_FLAG, VERBOSE_FLAG, ABORT_ON_FALSE_FLAG, ABORT_ON_TRUE_FLAG, INVERSE_FLAG];

    private static readonly string ABORT_COMMAND = "/macrocancel";

    private IChatGui ChatGui { get; init; } = chatGui;
    private ChatSender ChatSender { get; init; } = chatSender;
    private ICondition Condition { get; init; } = condition;

    protected override void Handler(string command, string args)
    {
        var parsedArgs = Arguments.SplitCommandLine(args);

        var flagArgs = parsedArgs.TakeWhile(a => a.StartsWith('-'));
        var commandArgs = parsedArgs[flagArgs.Count()..];

        var flags = flagArgs.SelectMany(a => a[1..].ToCharArray()).ToHashSet();
        if (flags.Except(KNOWN_FLAGS).Any())
        {
            ChatGui.PrintError(CommandHelpMessage);
            return;
        }

        var isDryRun = flags.Contains(DRY_RUN_FLAG);
        if (Condition[ConditionFlag.InThatPosition] ^ flags.Contains(INVERSE_FLAG))
        {
            foreach (var commandArg in commandArgs)
            {
                if (flags.Contains(VERBOSE_FLAG) || isDryRun)
                {
                    ChatGui.Print(commandArg);
                }

                if (!isDryRun)
                {
                    ChatSender.SendMessage(commandArg);
                }
            }

            if (flags.Contains(ABORT_ON_TRUE_FLAG))
            {
                SendAbort(flags);
            }
        } 
        else if (flags.Contains(ABORT_ON_FALSE_FLAG))
        {
            SendAbort(flags);
        }
    }

    private void SendAbort(HashSet<char> flags)
    {
        var isDryRun = flags.Contains(DRY_RUN_FLAG);
        if (flags.Contains(VERBOSE_FLAG) || isDryRun)
        {
            ChatGui.Print(ABORT_COMMAND);
        }

        if (!isDryRun)
        {
            ChatSender.SendMessage(ABORT_COMMAND);
        }
    }
}
