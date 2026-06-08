using FTPClient.Models.Models;
using FTPClient.Service.Abstractions;
using Renci.SshNet.Sftp;
using Directory = FTPClient.Models.Directory;

namespace FTPClient.Service.Interfaces;

public interface IServerOperationService
{
    Task<Result<IRemoteSession>> ConnectToServer(string Host, string Username, string Password, int Port, CancellationToken token);
    Result Disconnect(IRemoteSession session);
    Task<Result<IEnumerable<ISftpFile>>>? GetDirectoryContents(IRemoteSession session, string path, CancellationToken cancellationToken = default);
    Task<Result> UploadFile(IRemoteSession session, FileStream fileStream, string fileName, CancellationToken token);
    Task<Result> DownloadFile(IRemoteSession session, string serverFilePath, FileStream fileStream, CancellationToken token);
    Task<Result> CreateDirectory(IRemoteSession session, string serverDirectoryPath, CancellationToken token);
    Task<Result> CreateFile(IRemoteSession session, string serverFilePath, CancellationToken token);
    Task<Result> DeleteFile(IRemoteSession session, string serverFilePath, CancellationToken token);
    Task<Result> DeleteDirectory(IRemoteSession session, string serverDirectoryPath, CancellationToken token);
    Task<Result> DeleteDirectoryRecursively(IRemoteSession session, string serverFilePath, CancellationToken token);
    Task<Result> Rename(IRemoteSession session, string serverPath, string newName, CancellationToken token);
    Task<Result<Directory>> LoadSingleLevel(IRemoteSession session, string path, CancellationToken token, string displayName = null);
    Result<bool> IsPathExists(IRemoteSession session, string path);
    Result<SftpFileAttributes?> GetAttributes(IRemoteSession session, string path);
    Result ChangeDirectory(IRemoteSession session, string path);
}