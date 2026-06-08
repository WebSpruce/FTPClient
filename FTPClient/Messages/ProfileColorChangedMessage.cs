using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FTPClient.Messages;

public sealed class ProfileColorChangedMessage : ValueChangedMessage<Color>
{
    public ProfileColorChangedMessage(Color newColor) : base(newColor){ }
}