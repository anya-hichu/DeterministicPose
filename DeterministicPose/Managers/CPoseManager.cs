using Dalamud.Plugin.Services;
using DeterministicPose.Chat;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace DeterministicPose.Managers;

public unsafe class CPoseManager(IChatGui chatGui, IClientState clientState, ChatSender chatSender)
{
    private static readonly string CPOSE_COMMAND = "/cpose";
    private static readonly int MAX_TRIES = 8;

    private IChatGui ChatGui { get; init; } = chatGui;
    private IClientState ClientState { get; init; } = clientState;
    private ChatSender ChatSender { get; init; } = chatSender;

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
            if (i > MAX_TRIES)
            {
                ChatGui.PrintError($"Failed to change pose index to {target}");
                break;
            }
            ChatSender.SendMessage(CPOSE_COMMAND);
        }
    }
}
