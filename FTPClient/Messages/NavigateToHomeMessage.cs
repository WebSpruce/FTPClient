using CommunityToolkit.Mvvm.Messaging.Messages;
using FTPClient.Models.Models;

namespace FTPClient.Messages;

public sealed class NavigateToHomeMessage : ValueChangedMessage<Connection?>
{
    public NavigateToHomeMessage(Connection? connection) : base(connection) { }
}