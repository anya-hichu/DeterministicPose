using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace DeterministicPose.Managers;

public unsafe class CPoseManager(IClientState clientState, ChatSender chatSender, IPluginLog pluginLog)
{
    private static readonly string CPOSE_COMMAND = "/cpose";

    private IClientState ClientState { get; init; } = clientState;
    private ChatSender ChatSender { get; init; } = chatSender;
    private IPluginLog PluginLog { get; init; } = pluginLog;

    public byte GetCurrentPoseIndex()
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
            ChatSender.SendMessage(CPOSE_COMMAND);
        }
    }
}
