using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using DeterministicPose.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace DeterministicPose.Cmds;

public unsafe class LocalSyncCmd(IChatGui chatGui, IClientState clientState, ICommandManager commandManager, IObjectTable objectTable, IPluginLog pluginLog, ITargetManager targetManager) : BaseAnimCmd(clientState, commandManager, COMMAND_NAME, COMMAND_HELP_MESSAGE, objectTable, targetManager)
{
    private static readonly string COMMAND_NAME = "/localsync";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} <source player>( <target player>)?";

    private static readonly int MAX_RETRIES = 40;
    private static readonly int RETRY_WAIT_MS = 25;

    private IChatGui ChatGui { get; init; } = chatGui;
    private IPluginLog PluginLog { get; init; } = pluginLog;

    private CancellationTokenSource? CancellationTokenSource { get; set; }

    protected override void Handler(string command, string args)
    {
        var parsedArgs = Arguments.SplitCommandLine(args);

        var numParsedArgs = parsedArgs.Length;
        if (numParsedArgs < 1 || numParsedArgs > 2)
        {
            ChatGui.PrintError(COMMAND_HELP_MESSAGE);
            return;
        }

        var sourceName = parsedArgs[0];
        var sourcePlayer = FindPlayerCharacter(sourceName);

        if (sourcePlayer == null)
        {
            ChatGui.PrintError($"Failed to locate source '{sourceName}'");
            return;
        }

        var targetName = numParsedArgs > 1 ? parsedArgs[1] : "<me>";
        var targetPlayer = FindPlayerCharacter(targetName);

        if (targetPlayer == null)
        {
            ChatGui.PrintError($"Failed to locate target '{targetName}'");
            return;
        }

        var token = HandleCancellation();
        Task.Run(() =>
        {
            var success = false;
            for (var retries = 0; !success && retries < MAX_RETRIES && !token.IsCancellationRequested; retries++)
            {
                if (TryGetAnimationLocalTime(sourcePlayer, out var localTime))
                {
                    if (TrySetAnimationLocalTime(targetPlayer, localTime!.Value))
                    {
                        success = true;
                        PluginLog.Info($"Successfully copied animation local time ({localTime}) from '{sourcePlayer.Name}' to '{targetPlayer.Name}'");
                    }
                }
                Thread.Sleep(RETRY_WAIT_MS);
            }

            if (token.IsCancellationRequested)
            {
                ChatGui.PrintError($"Cancelled animation local time copy from '{sourcePlayer.Name}' to '{targetPlayer.Name}'");
                return;
            }

            if (!success)
            {
                ChatGui.PrintError($"Failed to copy animation local time from '{sourcePlayer.Name}' to '{targetPlayer.Name}' after {MAX_RETRIES} tries (waited {MAX_RETRIES * RETRY_WAIT_MS} ms)");
                return;
            }
        }, token);
    }

    private CancellationToken HandleCancellation()
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource = new();
        return CancellationTokenSource.Token;
    }
}
