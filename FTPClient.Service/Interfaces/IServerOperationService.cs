using Renci.SshNet;
using Renci.SshNet.Sftp;
using Directory = FTPClient.Models.Directory;

namespace FTPClient.Service.Interfaces;

public interface IServerOperationService
{
    Task<SftpClient> ConnectToServer(string Host, string Username, string Password, int Port);
    SftpClient DisconnectFromServer(SftpClient sftpClient);
    Task<IEnumerable<ISftpFile>>? GetAllDirectories(SftpClient sftpClient, string path, CancellationToken cancellationToken = default);
    Task UploadFile(SftpClient sftpClient, FileStream fileStream, string fileName);
    Task DownloadFile(SftpClient sftpClient, string serverFilePath, FileStream fileStream);
    Task CreateDirectory(SftpClient sftpClient, string serverDirectoryPath);
    Task CreateFile(SftpClient sftpClient, string serverFilePath);
    Task DeleteDirectoryRecursively(SftpClient sftpClient, string serverFilePath);
    Task DeleteFile(SftpClient sftpClient, string serverFilePath);
    Task DeleteDirectory(SftpClient sftpClient, string serverDirectoryPath);
    Task Rename(SftpClient sftpClient, string serverPath, string newName);
    Task<Directory> LoadSingleLevel(SftpClient sftpClient, string path, string displayName = null);
}