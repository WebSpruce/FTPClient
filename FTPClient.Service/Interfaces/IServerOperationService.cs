using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace FTPClient.Service.Interfaces;

public interface IServerOperationService
{
    Task<SftpClient> ConnectToServer(SftpClient sftpClient, string Host, string Username, string Password, string Port);
    SftpClient DisconnectFromServer(SftpClient sftpClient);
    IEnumerable<ISftpFile>? GetAllDirectories(SftpClient sftpClient, string path);
}