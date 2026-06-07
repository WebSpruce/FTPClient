using FTPClient.Models.Models;
using FTPClient.Service.Abstractions;
using Renci.SshNet;

namespace FTPClient.Session;

public class SessionConnection : ISessionConnection
{
    public Connection? CurrentConnection { get; set; }
    public IRemoteSession? CurrentSftpClient { get; set; }
    public bool IsConnected => CurrentSftpClient?.IsConnected ?? false;

    public void ClearSession()
    {
        CurrentConnection = null;
        CurrentSftpClient?.DisposeAsync();
        CurrentSftpClient = null;
    }
}