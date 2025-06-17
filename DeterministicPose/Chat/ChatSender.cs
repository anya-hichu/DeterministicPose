using Dalamud.Plugin.Services;
using System.Threading.Tasks;

namespace DeterministicPose.Chat;

public class ChatSender(ChatServer chatServer, IPluginLog pluginLog)
{
    public ChatServer ChatServer { get; init; } = chatServer;
    public IPluginLog PluginLog { get; init; } = pluginLog;

    public void SendMessage(string message)
    {
        ChatServer.SendMessage(message);
        PluginLog.Verbose($"Sent chat message: '{message}'");
    }
}
