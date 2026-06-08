using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FTPClient.Messages;

public sealed class ProfileNameChangedMessage : ValueChangedMessage<string>
{
    public ProfileNameChangedMessage(string newName) : base(newName) { }
}