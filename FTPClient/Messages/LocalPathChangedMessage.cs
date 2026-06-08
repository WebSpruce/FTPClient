using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FTPClient.Messages;

public sealed class LocalPathChangedMessage : ValueChangedMessage<string>
{
    public LocalPathChangedMessage(string localPath) : base(localPath) { }
}