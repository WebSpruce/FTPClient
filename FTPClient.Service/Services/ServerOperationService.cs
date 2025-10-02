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
    public void UploadFile(SftpClient sftpClient, FileStream fileStream, string fileName)
    {
        sftpClient.BufferSize = 4 * 1024;
        sftpClient.UploadFile(fileStream, fileName);
    }
    public void DeleteFile(SftpClient sftpClient, string serverFilePath)
    {
        sftpClient.DeleteFile(serverFilePath);
    } 
    public void DeleteDirectory(SftpClient sftpClient, string serverDirectoryPath)
    {
        sftpClient.DeleteDirectory(serverDirectoryPath);
    } 
    public void CreateDirectory(SftpClient sftpClient, string serverDirectoryPath)
    {
        sftpClient.CreateDirectory(serverDirectoryPath);
    } 
    public void CreateFile(SftpClient sftpClient, string serverFilePath)
    {
        sftpClient.Create(serverFilePath);
    } 
    public void DownloadFile(SftpClient sftpClient, string serverFilePath, FileStream fileStream)
    {
        sftpClient.DownloadFile(serverFilePath, fileStream);
    }
    public async Task Rename(SftpClient sftpClient, string serverPath, string newName)
    {
        try
        {
            var cancellationToken = new CancellationTokenSource();
            var lastSlashIndex = serverPath.LastIndexOf("/");
            var dur = serverPath.Length - lastSlashIndex;
            var pathWithoutFile = serverPath.Remove(lastSlashIndex, dur);
            var newFilePath = $"{pathWithoutFile}/{newName}";
            await sftpClient.RenameFileAsync(serverPath, newFilePath, cancellationToken.Token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilesAndDirectoriesService Rename error: {ex}");
        }
    }
}