using System.Diagnostics;
using FTPClient.Service.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace FTPClient.Service.Services;

public class ServerOperationService : IServerOperationService
{
    public async Task<SftpClient> ConnectToServer(SftpClient sftpClient, string Host, string Username, string Password, string Port)
    {
        try
        {
            sftpClient = new SftpClient(new PasswordConnectionInfo(Host, int.Parse(Port), Username, Password));
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

    public IEnumerable<ISftpFile>? GetAllDirectories(SftpClient sftpClient, string path)
    {
        try
        {
            return sftpClient.ListDirectory(path); 
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
    public void DownloadFile(SftpClient sftpClient, string serverFilePath, FileStream fileStream)
    {
        sftpClient.DownloadFile(serverFilePath, fileStream);
    }
}