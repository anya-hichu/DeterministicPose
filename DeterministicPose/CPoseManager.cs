using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;

namespace DeterministicPose;

public unsafe class CPoseManager
{
    private Character* PlayerCharacter { get; init; }
    private Chat Chat { get; init; }
    private IPluginLog PluginLog { get; init; }

    public CPoseManager(IClientState clientState, Chat chat, IPluginLog pluginLog)
    {
        var player = clientState.LocalPlayer;
        PlayerCharacter = (Character*)(player?.Address ?? IntPtr.Zero);

        Chat = chat;
        PluginLog = pluginLog;
    }

    private byte GetCurrentPoseIndex()
    {
        return PlayerCharacter->EmoteController.CPoseState;
    }

    public void Change(byte target)
    {
        for(var i = 0;GetCurrentPoseIndex() != target; i++)
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
