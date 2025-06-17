using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using DeterministicPose.Managers;
using DeterministicPose.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DeterministicPose.Cmds;

public unsafe class RemoteSyncCmd(IChatGui chatGui, ChatSender chatSender, IClientState clientState, CPoseManager cPoseManager, ICommandManager commandManager, ExcelSheet<Emote> emoteSheet, IObjectTable objectTable, IPluginLog pluginLog, ITargetManager targetManager) : BaseAnimCmd(clientState, commandManager, COMMAND_NAME, COMMAND_HELP_MESSAGE, objectTable, targetManager)
{
    private static readonly string COMMAND_NAME = "/remotesync";
    private static readonly string COMMAND_HELP_MESSAGE = $"Command usage: {COMMAND_NAME} (<player name>( delay)?|cancel)";

    private static readonly float MIN_SYNC_DELTA = 0.1f;
    private static readonly int WAIT_INTERVAL_MS = 10;
    private static readonly int MAX_WAIT_MS = 30_000;

    private IChatGui ChatGui { get; init; } = chatGui;
    private ChatSender ChatSender { get; init; } = chatSender;
    private CPoseManager CPoseManager { get; init; } = cPoseManager;
    private ExcelSheet<Emote> EmoteSheet { get; init; } = emoteSheet;
    private IPluginLog PluginLog { get; init; } = pluginLog;

    private CancellationTokenSource? CancellationTokenSource { get; set; }
    private Stopwatch Timer { get; init; } = new();

    public override void Dispose()
    {
        base.Dispose();
        CancellationTokenSource?.Cancel();
    }

    protected override void Handler(string command, string args)
    {
        var parsedArgs = Arguments.SplitCommandLine(args);
        var numParsedArgs = parsedArgs.Length;
        if (numParsedArgs < 1 || numParsedArgs > 3)
        {
            ChatGui.PrintError(COMMAND_HELP_MESSAGE);
            return;
        }

        var arg = parsedArgs[0];
        if (arg == "cancel")
        {
            CancellationTokenSource?.Cancel();
            ChatGui.Print($"Cancelled remove sync");
            return;
        }

        // Offset animation start
        var delay = 0;
        var hasDelay = numParsedArgs > 1 && int.TryParse(parsedArgs[2], out delay) && delay > 0;

        var player = FindPlayerCharacter(arg);
        if (player == null)
        {
            ChatGui.PrintError($"Failed to locate player '{arg}'");
            return;
        }

        var localPlayer = ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            ChatGui.PrintError($"No local player");
            return;
        }

        var localCharacter = (Character*)localPlayer.Address;
        if (localCharacter->Mode is CharacterModes.InPositionLoop)
        {
            ChatGui.PrintError($"In position emotes are not supported currently");
            return;
        }

        var emoteId = localCharacter->EmoteController.EmoteId;
        if (emoteId == 0)
        {
            ChatGui.PrintError("Local player is not executing an emote");
            return;
        }

        if (!EmoteSheet.TryGetRow(emoteId, out var emote))
        {
            ChatGui.PrintError($"Could not find emote with id {emoteId}");
            return;
        }

        var textCommand = emote.TextCommand;
        if (!textCommand.IsValid)
        {
            ChatGui.PrintError($"Could not find associated command for emote {emote.Name}");
            return;
        }

        var token = HandleCancellation();
        Task.Run(() =>
        {
            float? localTime = null;
            var success = false;

            Timer.Restart();
            while (!success && !token.IsCancellationRequested && Timer.ElapsedMilliseconds < MAX_WAIT_MS)
            {
                if (TryGetAnimationLocalTime(player, out localTime) && localTime!.Value < MIN_SYNC_DELTA)
                {
                    success = true;
                } 
                else
                {
                    Thread.Sleep(WAIT_INTERVAL_MS);
                }
            }
            Timer.Stop();

            if (success)
            {
                if (hasDelay)
                {
                    Thread.Sleep(delay);
                }
                ChatSender.SendMessage($"{textCommand.Value.Command} motion");
            } 
            else
            {
                PluginLog.Error($"Failed to sync after {Timer.ElapsedMilliseconds} ms");
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
