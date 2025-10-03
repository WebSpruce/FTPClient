using System.Diagnostics;
using FTPClient.Service.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace FTPClient.Service.Services;

public class ServerOperationService : IServerOperationService
{
    public async Task<SftpClient> ConnectToServer(string host, string username, string password, int port)
    {
        try
        {
            var connectionInfo = new PasswordConnectionInfo(host, port, username, password)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            var sftpClient = new SftpClient(connectionInfo);
            await Task.Run(() => sftpClient.Connect());
            return sftpClient;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService ConnectToServer error: {ex}");
            return new SftpClient(String.Empty, String.Empty);
        }
    }

    public SftpClient DisconnectFromServer(SftpClient sftpClient)
    {
        try
        {
            sftpClient.Disconnect();
            return sftpClient;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService DisconnectFromServer error: {ex}");
            return new SftpClient(String.Empty, String.Empty);
        }
    }

    public async Task<IEnumerable<ISftpFile>> GetAllDirectories(SftpClient sftpClient, string path, CancellationToken cancellationToken = default)
    {
        try
        {
            return await sftpClient.ListDirectoryAsync(path, cancellationToken).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService GetAllDirectories error: {ex}");
            return new List<ISftpFile>();
        }
    }
    public async Task UploadFile(SftpClient sftpClient, FileStream fileStream, string fileName)
    {
        sftpClient.BufferSize = 4 * 1024;
        await Task.Run(() => sftpClient.UploadFile(fileStream, fileName));
    }
    public async Task DeleteFile(SftpClient sftpClient, string serverFilePath)
    {
        await Task.Run(() => sftpClient.DeleteFile(serverFilePath));
    } 
    public async Task DeleteDirectory(SftpClient sftpClient, string serverDirectoryPath)
    {
        await Task.Run(() => sftpClient.DeleteDirectory(serverDirectoryPath));
    } 
    public async Task CreateDirectory(SftpClient sftpClient, string serverDirectoryPath)
    {
        await Task.Run(() =>sftpClient.CreateDirectory(serverDirectoryPath));
    } 
    public async Task CreateFile(SftpClient sftpClient, string serverFilePath)
    {
        await Task.Run(() => sftpClient.Create(serverFilePath));
    } 
    public async Task DownloadFile(SftpClient sftpClient, string serverFilePath, FileStream fileStream)
    {
        await Task.Run(() => sftpClient.DownloadFile(serverFilePath, fileStream));
    }
    public async Task Rename(SftpClient sftpClient, string serverPath, string newName)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(serverPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = "/";
            }

            var newFilePath = $"{directoryPath}/{newName}";

            var token = new CancellationToken();
            await sftpClient.RenameFileAsync(serverPath, newFilePath, token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilesAndDirectoriesService Rename error: {ex}");
            throw;
        }
    }
}