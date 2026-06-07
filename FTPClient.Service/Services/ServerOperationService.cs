using System.Collections.ObjectModel;
using System.Diagnostics;
using FTPClient.Models;
using FTPClient.Models.Models;
using FTPClient.Service.Abstractions;
using FTPClient.Service.Helper;
using FTPClient.Service.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Directory = FTPClient.Models.Directory;

namespace FTPClient.Service.Services;

public class ServerOperationService : IServerOperationService
{
    public async Task<Result<IRemoteSession>> ConnectToServer(string host, string username, string password, int port,
        CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled<IRemoteSession>();
            
            var connectionInfo = new PasswordConnectionInfo(host, port, username, password)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            var sftpClient = new SftpClient(connectionInfo);
            await sftpClient.ConnectAsync(token);

            return Result.Success<IRemoteSession>(new SftpRemoteSession(sftpClient));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService ConnectToServer error: {ex.Message} - {ex.InnerException}");
            return Result.Failure<IRemoteSession>([$"ConnectToServer error: {ex.Message} - {ex.InnerException}"]);
        }
    }

    public Result Disconnect(IRemoteSession session)
    {
        try
        {
            var client = ((SftpRemoteSession)session).Client;
            client.Disconnect();
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService Disconnect error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"Disconnect error: {ex.Message} - {ex.InnerException}"]);
        }
    }

    public async Task<Result<IEnumerable<ISftpFile>>> GetDirectoryContents(IRemoteSession session, string path, CancellationToken token = default)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled<IEnumerable<ISftpFile>>();
            var client = ((SftpRemoteSession)session).Client;
            var list = await client.ListDirectoryAsync(path, token).ToListAsync(token);
            return Result.Success<IEnumerable<ISftpFile>>(list);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService GetDirectoryContents error: {ex.Message} - {ex.InnerException}");
            return Result.Failure<IEnumerable<ISftpFile>>([$"GetDirectoryContents error: {ex.Message} - {ex.InnerException}"]);
        }
    }
    public async Task<Result> UploadFile(IRemoteSession session, FileStream fileStream, string fileName,
        CancellationToken token)
    {
        try
        {
            if (token.IsCancellationRequested)
                return Result.Cancelled();
        
            var client = ((SftpRemoteSession)session).Client;
            client.BufferSize = 4 * 1024;
            await Task.Run(() => client.UploadFile(fileStream, fileName), token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService UploadFile error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"UploadFile error: {ex.Message} - {ex.InnerException}"]);
        }
    }
    public async Task<Result> DeleteDirectoryRecursively(IRemoteSession session, string path,
        CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled();
            var client = ((SftpRemoteSession)session).Client;
            var dirItems = await client.ListDirectoryAsync(path, token).ToListAsync(token);

            foreach (var item in dirItems)
            {
                if(item.Name == "." || item.Name == "..") continue;

                if (item.IsDirectory)
                    await DeleteDirectoryRecursively(session, item.FullName, token);
                else
                    await DeleteFile(session, item.FullName, token);
            }

            await Task.Run(() => client.DeleteDirectory(path), token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService DeleteDirectoryRecursively error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"DeleteDirectoryRecursively error: {ex.Message} - {ex.InnerException}"]);
        }
    }
    public async Task<Result> DeleteFile(IRemoteSession session, string serverFilePath,
         CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled();
            var client = ((SftpRemoteSession)session).Client;
            await client.DeleteFileAsync(serverFilePath, token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService DeleteFile error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"DeleteFile error: {ex.Message} - {ex.InnerException}"]);
        }
    } 
    public async Task<Result> DeleteDirectory(IRemoteSession session, string serverDirectoryPath,
        CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled();
            var client = ((SftpRemoteSession)session).Client;
            await Task.Run(() => client.DeleteDirectory(serverDirectoryPath), token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService DeleteDirectory error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"DeleteDirectory error: {ex.Message} - {ex.InnerException}"]);
        }
        
    } 
    public async Task<Result> CreateDirectory(IRemoteSession session, string serverDirectoryPath,
        CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled();
            var client = ((SftpRemoteSession)session).Client;
            await Task.Run(() => client.CreateDirectory(serverDirectoryPath), token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService CreateDirectory error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"CreateDirectory error: {ex.Message} - {ex.InnerException}"]);
        }
        
    } 
    public async Task<Result> CreateFile(IRemoteSession session, string serverFilePath,
        CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled();
            var client = ((SftpRemoteSession)session).Client;
            await Task.Run(() => client.Create(serverFilePath), token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService CreateFile error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"CreateFile error: {ex.Message} - {ex.InnerException}"]);
        }
    } 
    public async Task<Result> DownloadFile(IRemoteSession session, string serverFilePath, FileStream fileStream,
        CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled();
            var client = ((SftpRemoteSession)session).Client;
            await Task.Run(() => client.DownloadFile(serverFilePath, fileStream), token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService DownloadFile error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"DownloadFile error: {ex.Message} - {ex.InnerException}"]);
        }
    }
    public async Task<Result> Rename(IRemoteSession session, string serverPath, string newName,
        CancellationToken token)
    {
        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled();
            var client = ((SftpRemoteSession)session).Client;
            var directoryPath = Path.GetDirectoryName(serverPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = "/";
            }

            var newFilePath = $"{directoryPath}/{newName}";

            await client.RenameFileAsync(serverPath, newFilePath, token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOperationService Rename error: {ex.Message} - {ex.InnerException}");
            return Result.Failure([$"Rename error: {ex.Message} - {ex.InnerException}"]);
        }
    }
    public async Task<Result<Directory>> LoadSingleLevel(IRemoteSession session, string path, 
        CancellationToken token, string displayName = null)
    {
        var directory = new Directory()
        {
            Name = displayName ?? Path.GetFileName(path) ?? path,
            Path = path,
            Size = "",
            ChildrenLoaded = false,
            HasChildren = true,
            IsLoading = false
        };

        try
        {
            if(token.IsCancellationRequested)
                return Result.Cancelled<Directory>();
            
            var getDirectoryContentsResult = await GetDirectoryContents(session, path, token);
            if (!getDirectoryContentsResult.IsSuccess)
                return Result.Failure<Directory>(getDirectoryContentsResult.Errors);

            var items = getDirectoryContentsResult.Value.ToList();
            
            var subDirToProcess = items
                .Where(item => item.IsDirectory && item.Name != "." && item.Name != "..")
                .ToList();
            var filesInDir = items.Where(item => !item.IsDirectory).ToList();

            foreach (var file in filesInDir)
            {
                directory.FileItems.Add(new FileItem 
                { 
                    Name = Path.GetFileName(file.FullName), 
                    Path = file.FullName,
                    Size = DataFormating.FormatFileSize(file.Attributes.Size) 
                });
            }
            // show only placeholders, loading on demand
            foreach (var sub in subDirToProcess)
            {
                var placeholder = new Directory
                {
                    Name = sub.Name,
                    Path = sub.FullName,
                    Size = "",
                    HasChildren = true,
                    ChildrenLoaded = false
                };
                // Seed with one child
                placeholder.FileItems.Add(new FileItem { Name = "⏳", Path = null, Size = "" });
                directory.FileItems.Add(placeholder);
            }

            directory.FileItems = new ObservableCollection<FileItem>(
                directory.FileItems
                    .OrderBy(x => x.Name)
                    .ToList()
                );
            directory.ChildrenLoaded = true;
            directory.HasChildren = subDirToProcess.Any() || filesInDir.Any();
        }
        catch (SftpPermissionDeniedException ex)
        {
            Debug.WriteLine($"Permission denied for directory: {path}. {ex.Message}");
            return Result.Failure<Directory>([$"LoadSingleLevel Sftp error: {ex.Message} - {ex.InnerException}"]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading directory {path}: {ex.Message} - {ex.InnerException}");
            return Result.Failure<Directory>([$"LoadSingleLevel error: {ex.Message} - {ex.InnerException}"]);
        }

        return Result.Success(directory);
    }

    public Result<bool> IsPathExists(IRemoteSession session, string path)
    {
        try
        {
            var client = ((SftpRemoteSession)session).Client;
            return Result.Success(client.Exists(path));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>([$"IsPathExists error: {ex.Message} - {ex.InnerException}"]);
        }
    }

    public Result<SftpFileAttributes?> GetAttributes(IRemoteSession session, string path)
    {
        try
        {
            var client = ((SftpRemoteSession)session).Client;

            return Result.Success(client.GetAttributes(path));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOpeartionService GetAttributes error: {ex.Message} - {ex.InnerException}");
            return Result.Failure<SftpFileAttributes?>([$"GetAttributes error: {ex.Message} - {ex.InnerException}"]);
        }
    }

    public Result ChangeDirectory(IRemoteSession session, string path)
    {
        try
        {
            var client = ((SftpRemoteSession)session).Client;
            client.ChangeDirectory(path);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ServerOpeartionService ChangeDirectory error: {ex.Message} - {ex.InnerException}");
            return Result.Failure<SftpFileAttributes?>([$"ChangeDirectory error: {ex.Message} - {ex.InnerException}"]);
        }
    }
}