using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace DeterministicPose;

public unsafe class CPoseManager(IClientState clientState, Chat chat, IPluginLog pluginLog)
{
    private IClientState ClientState { get; init; } = clientState;
    private Chat Chat { get; init; } = chat;
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
        for(var i = 0; GetCurrentPoseIndex() != target; i++)
        {
            if (i > 8)
            {
                PluginLog.Error("Could not change pose from {current} to {target}", GetCurrentPoseIndex(), target);
                break;
            }
            Chat.SendMessage("/cpose");
        }
    }
}
