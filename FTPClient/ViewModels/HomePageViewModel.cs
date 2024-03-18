using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using Renci.SshNet;
using Renci.SshNet.Common;
using Directory = FTPClient.Models.Directory;

namespace FTPClient.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{
    
    private string _host = string.Empty;
    public string Host
    {
        get => _host;
        set
        {
            if (Regex.IsMatch(value, @"^[\d\.:]*$"))
            {
                SetProperty(ref _host, value);
            }
        }
    }
    [ObservableProperty] 
    private string _username = string.Empty; 
    [ObservableProperty] 
    private string _password = string.Empty;
    private string _port = string.Empty;
    public string Port
    {
        get => _port;
        set
        {
            if (Regex.IsMatch(value, @"^\d*$"))
            {
                SetProperty(ref _port, value);
            }
        }
    }
    [ObservableProperty] 
    private bool _connectBtnVisibility = true;
    [ObservableProperty] 
    private bool _disconnectBtnVisibility;

    [ObservableProperty]
    private bool _isConnected=false;

    [ObservableProperty] 
    private double _serverProgressBarValue;
    [ObservableProperty] 
    private ObservableCollection<Directory> _serverFiles = new();
    [ObservableProperty] 
    private string _serverPath = "/";

    [ObservableProperty]
    private double _localProgressBarValue;
    [ObservableProperty]
    private ObservableCollection<Directory> _localFiles = new();
    [ObservableProperty]
    private string _localPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private SftpClient sftpClient;
    
    private Directory _selectedDirectory;
    public Directory SelectedDirectory
    {
        get => _selectedDirectory;
        set
        {
            SetProperty(ref _selectedDirectory, value);
            if (_selectedDirectory != null)
            {
                SelectedFileItem = null;
                ServerPath = _selectedDirectory.Path;
            }
        }
    }
    private FileItem _selectedFileItem;
    public FileItem SelectedFileItem
    {
        get => _selectedFileItem;
        set
        {
            SetProperty(ref _selectedFileItem, value);
            if (_selectedFileItem != null)
            {
                SelectedDirectory = null;
                ServerPath = _selectedFileItem.Path;
            }
        }
    }
    private object _selectedServerItem;
    public object SelectedServerItem
    {
        get => _selectedServerItem;
        set
        {
            if (value is Directory directory)
            {
                SelectedDirectory = directory;
            }
            else if (value is FileItem fileItem)
            {
                SelectedFileItem = fileItem;
            }
        }
    }

    private Directory _selectedLocalDirectory;
    public Directory SelectedLocalDirectory
    {
        get => _selectedLocalDirectory;
        set
        {
            SetProperty(ref _selectedLocalDirectory, value);
            if (_selectedLocalDirectory != null)
            {
                SelectedLocalFileItem = null;
                LocalPath = _selectedLocalDirectory.Path;
            }
        }
    }
    private FileItem _selectedLocalFileItem;
    public FileItem SelectedLocalFileItem
    {
        get => _selectedLocalFileItem;
        set
        {
            SetProperty(ref _selectedLocalFileItem, value);
            if (_selectedLocalFileItem != null)
            {
                SelectedLocalDirectory = null;
                LocalPath = _selectedLocalFileItem.Path;
            }
        }
    }
    private object _selectedLocalItem;
    public object SelectedLocalItem
    {
        get => _selectedLocalItem;
        set
        {
            if (value is Directory directory)
            {
                SelectedLocalDirectory = directory;
            }
            else if (value is FileItem fileItem)
            {
                SelectedLocalFileItem = fileItem;
            }
        }
    }


    private readonly IServerOperationService _serverOperationService;
    public HomePageViewModel()
    {
        _serverOperationService = ((App)Application.Current).Services.GetRequiredService<IServerOperationService>();
        LocalFiles.Add(ShowDirectoriesAndFilesDefault(LocalPath));
    }

    public HomePageViewModel(IServerOperationService serverOperationService)
    {
        _serverOperationService = serverOperationService;
        LocalFiles.Add(ShowDirectoriesAndFilesDefault(LocalPath));
    }
    
    [RelayCommand]
    private async Task ConnectToServer()
    {
        try
        {
            ServerProgressBarValue = 0;
            if (string.IsNullOrEmpty(Host))
            {
                var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Host input is null or empty.");
                await errorMessageBox.ShowAsync();
            }
            else if (string.IsNullOrEmpty(Username))
            {
                var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Username input is null or empty.");
                await errorMessageBox.ShowAsync();
            }
            else if (string.IsNullOrEmpty(Password))
            {
                var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Password input is null or empty.");
                await errorMessageBox.ShowAsync();
            }
            else if (string.IsNullOrEmpty(Port))
            {
                var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Port input is null or empty.");
                await errorMessageBox.ShowAsync();
            }
            else if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Username) &&
                     !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(Port))
            {
                sftpClient = await _serverOperationService.ConnectToServer(sftpClient, Host,  Username, Password, Port);
                if (sftpClient.IsConnected)
                {
                    await Task.Run(() => ServerFiles.Add(GetAllDirectories(sftpClient, "/")));
                }
                
                var connectedMessageBox = MessageBoxManager.GetMessageBoxStandard("Success!", "Connected to the server.");
                await connectedMessageBox.ShowAsync();
                
                SetBtnsVisibility();
                IsConnected = true;
            }

        }
        catch (SocketException se)
        {
            Debug.WriteLine($"Unreachable network : {se}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Unreachable network.");
            await errorMessageBox.ShowAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connect to the server error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Couldn't connect to the server.");
            await errorMessageBox.ShowAsync();
        }
        finally
        {
            ServerProgressBarValue = 100;
        }
    }
    [RelayCommand]
    private void DisconnectFromServer()
    {
        try
        {
            if (sftpClient.IsConnected)
            {
                sftpClient = _serverOperationService.DisconnectFromServer(sftpClient);
                if (!sftpClient.IsConnected)
                {
                    ServerFiles.Clear();
                    ServerPath = "/";
                }
            }
            SetBtnsVisibility();
            IsConnected = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Disconnect from the server error : {ex}");
        }
    }

    private void SetBtnsVisibility()
    {
        ConnectBtnVisibility = !ConnectBtnVisibility;
        DisconnectBtnVisibility = !DisconnectBtnVisibility;
    }

    private Directory GetAllDirectories(SftpClient sftpClient, string path)
    {
        var directory = new Directory { Name = Path.GetFileName(path), Path = path };
        try
        {
            var listOfFiles = _serverOperationService.GetAllDirectories(sftpClient, path);
            ServerProgressBarValue = 50;
            foreach (var file in listOfFiles)
            {
                if (file.IsDirectory && !file.Name.StartsWith(".") && !file.Name.StartsWith(".."))
                {
                    directory.FileItems.Add(GetAllDirectories(sftpClient, file.FullName));
                }else if (!file.IsDirectory && !file.Name.StartsWith(".") && !file.Name.StartsWith(".."))
                {
                    directory.FileItems.Add(new FileItem { Name = Path.GetFileName(file.FullName), Path = file.FullName });
                }
            }
        }
        catch (SftpPermissionDeniedException ex)
        {
            Debug.WriteLine($"Permission denied for directory: {path}. {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllDirectories error: {ex}");
        }

        return directory;
    }

    private Directory ShowDirectoriesAndFilesDefault(string path)
    {
        var directory = new Directory { Name = Path.GetFileName(path), Path = path };
        try
        {
            var listOfFiles = System.IO.Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            foreach (var file in listOfFiles)
            {
                var fileName = Path.GetFileName(file);
                file.Replace("file:///", "");
                if (System.IO.Directory.Exists(file) && !fileName.StartsWith(".") && !fileName.StartsWith(".."))
                {
                    directory.FileItems.Add(ShowDirectoriesAndFilesDefault(file));
                }
                else if (!System.IO.Directory.Exists(file) && !fileName.StartsWith(".") && !fileName.StartsWith(".."))
                {
                    directory.FileItems.Add(new FileItem { Name = fileName, Path = file });
                }
            }
        }
        catch (SftpPermissionDeniedException ex)
        {
            Debug.WriteLine($"Permission denied for directory: {path}. {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllDirectories error: {ex}");
        }
        return directory;
    }
    [RelayCommand]
    private async void OpenFolder()
    {
        try
        {
            var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select file",
                AllowMultiple = true,
            });

            if (files.Count > 0)
            {
                LocalFiles.Clear();
                foreach (var file in files)
                {
                    LocalFiles.Add(new Directory() { Name = file.Name, Path = file.Path.AbsolutePath.ToString() });
                }
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Open file error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't open the file.");
            await errorMessageBox.ShowAsync();
        }
        
    }
    [RelayCommand]
    private async Task RemoveFromServer()
    {
        try
        {
            if (sftpClient.IsConnected)
            {
                sftpClient.DeleteFile(ServerPath);

                var removeFileMessageBox = MessageBoxManager.GetMessageBoxStandard("Success!", "Files have just been removed.");
                await removeFileMessageBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Remove file error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't delete the file.");
            await errorMessageBox.ShowAsync();
        }
    } 
    [RelayCommand]
    private async Task MoveToServer()
    {
        try
        {
            if (sftpClient.IsConnected)
            {
                sftpClient.ChangeDirectory(ServerPath);
                foreach (var file in LocalFiles)
                {
                    using (var fileStream = new FileStream(file.Path, FileMode.Open))
                    {
                        Debug.WriteLine($"file: {file.Path}, {file.Name} - {fileStream.Length}");
                        sftpClient.BufferSize = 4 * 1024;
                        sftpClient.UploadFile(fileStream, Path.GetFileName(file.Path));
                    }
                }

                var uploadFileMessageBox = MessageBoxManager.GetMessageBoxStandard("Success!", "Files have just been uploaded to the server.");
                await uploadFileMessageBox.ShowAsync();
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Upload file error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't upload the file.");
            await errorMessageBox.ShowAsync();
        }
    } 
    [RelayCommand]
    private async Task MoveToLocal()
    {
        try
        {
            if (sftpClient.IsConnected)
            {
                var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

                var directory = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select folder",
                    AllowMultiple = false,
                });

                if (directory.Count > 0)
                {
                    foreach (var file in directory)
                    {
                        using (var fileStream = File.Create(file.Path.AbsolutePath.ToString()))
                        {
                            sftpClient.DownloadFile(ServerPath, fileStream);
                        }
                    }
                }
                
                var downloadFileMessageBox = MessageBoxManager.GetMessageBoxStandard("Success!", "Files have just been downloaded from the server.");
                await downloadFileMessageBox.ShowAsync();
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Download file error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't download the file.");
            await errorMessageBox.ShowAsync();
        }
        
    }
}