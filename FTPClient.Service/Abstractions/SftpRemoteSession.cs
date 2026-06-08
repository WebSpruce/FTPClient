using Renci.SshNet;

namespace FTPClient.Service.Abstractions;

public class SftpRemoteSession : IRemoteSession
{
    internal SftpClient Client { get; } // only ServerOperationService uses it
    public bool IsConnected => Client.IsConnected;
    
    public SftpRemoteSession(SftpClient client)
    {
        Client = client;
    }
    
    public async ValueTask DisposeAsync()
    {
        if(Client.IsConnected)
            Client.Disconnect();
        Client.Dispose();
    }
}