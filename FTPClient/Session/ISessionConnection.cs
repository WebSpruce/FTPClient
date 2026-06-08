using FTPClient.Models.Models;
using FTPClient.Service.Abstractions;
using Renci.SshNet;

namespace FTPClient.Session;

public interface ISessionConnection
{
    Connection? CurrentConnection { get; set; }
    IRemoteSession? CurrentSftpClient { get; set; }
    bool IsConnected { get; }
    void ClearSession();
}