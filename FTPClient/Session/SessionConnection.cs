using FTPClient.Models.Models;
using Renci.SshNet;

namespace FTPClient.Session;

public class SessionConnection
{
    private static SessionConnection? _instance;
    public static SessionConnection Instance => _instance ??= new SessionConnection();
    public Connection? CurrentConnection { get; set; }
    public SftpClient? CurrentSftpClient { get; set; }
    public bool IsConnected => CurrentSftpClient?.IsConnected ?? false;

    public void ClearSession()
    {
        CurrentConnection = null;
        CurrentSftpClient?.Dispose();
        CurrentSftpClient = null;
    }
}