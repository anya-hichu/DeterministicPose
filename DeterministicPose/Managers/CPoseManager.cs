using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace DeterministicPose.Managers;

public unsafe class CPoseManager(IClientState clientState, ChatServer chatServer, IPluginLog pluginLog)
{
    private static readonly string CPOSE_COMMAND = "/cpose";

    private IClientState ClientState { get; init; } = clientState;
    private ChatServer ChatServer { get; init; } = chatServer;
    private IPluginLog PluginLog { get; init; } = pluginLog;

    private byte GetCurrentPoseIndex()
    {
        var player = ClientState.LocalPlayer;

        if (player != null)
        {
            var character = (Character*)player.Address;
            return character->EmoteController.CPoseState;
        }
        else
        {
            return 0;
        }
    }

    public void Change(byte target)
    {
        for (var i = 0; GetCurrentPoseIndex() != target; i++)
        {
            if (i > 8)
            {
                PluginLog.Error($"Could not change pose from {GetCurrentPoseIndex()} to {target}");
                break;
            }
            ChatServer.SendMessage(CPOSE_COMMAND);
        }
    }
}
