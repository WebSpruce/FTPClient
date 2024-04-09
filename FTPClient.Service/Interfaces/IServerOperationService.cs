using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace FTPClient.Service.Interfaces;

public interface IServerOperationService
{
    Task<SftpClient> ConnectToServer(SftpClient sftpClient, string Host, string Username, string Password, string Port);
    SftpClient DisconnectFromServer(SftpClient sftpClient);
    Task<IEnumerable<ISftpFile>>? GetAllDirectories(SftpClient sftpClient, string path);
    void UploadFile(SftpClient sftpClient, FileStream fileStream, string fileName);
    void DownloadFile(SftpClient sftpClient, string serverFilePath, FileStream fileStream);
    void CreateDirectory(SftpClient sftpClient, string serverDirectoryPath);
    void CreateFile(SftpClient sftpClient, string serverFilePath);
    void DeleteFile(SftpClient sftpClient, string serverFilePath);
    void DeleteDirectory(SftpClient sftpClient, string serverDirectoryPath);
    Task Rename(SftpClient sftpClient, string serverPath, string newName);
}