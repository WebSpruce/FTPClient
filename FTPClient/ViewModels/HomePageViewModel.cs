using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Database.Interfaces;
using FTPClient.Models;
using FTPClient.Models.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Views;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Directory = FTPClient.Models.Directory;
using File = System.IO.File;
using Path = System.IO.Path;

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

    internal string currentProfileName = string.Empty;

    [ObservableProperty]
    private string _newDirectoryName = string.Empty;
    [ObservableProperty]
    private string _newFileName = string.Empty;
    [ObservableProperty]
    private string _newName = string.Empty;


    private readonly IServerOperationService _serverOperationService;
    private readonly IFilesAndDirectoriesService _filesAndDirectoriesService;
    private readonly IConnectionsRepository _connectionsRepository;
    public static HomePageViewModel instance;
    public HomePageViewModel()
    {
        instance = this;
        _serverOperationService = ((App)Application.Current).Services.GetRequiredService<IServerOperationService>();
        _filesAndDirectoriesService = ((App)Application.Current).Services.GetRequiredService<IFilesAndDirectoriesService>();
        _connectionsRepository = ((App)Application.Current).Services.GetRequiredService<IConnectionsRepository>();

        var currentProfileName = _filesAndDirectoriesService.GetCurrentProfile();
        var currentProfile = _filesAndDirectoriesService.GetUserSettings(currentProfileName);
        if(currentProfile.ProfileSettings != null)
        {
            LocalPath = currentProfile.ProfileSettings.LocalPath;
        }
        else
        {
            LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
    }
    public HomePageViewModel(Connection connection)
    {
        instance = this;
        _serverOperationService = ((App)Application.Current).Services.GetRequiredService<IServerOperationService>();
        _filesAndDirectoriesService = ((App)Application.Current).Services.GetRequiredService<IFilesAndDirectoriesService>();
        _connectionsRepository = ((App)Application.Current).Services.GetRequiredService<IConnectionsRepository>();

        var currentProfileName = _filesAndDirectoriesService.GetCurrentProfile();
        var currentProfile = _filesAndDirectoriesService.GetUserSettings(currentProfileName);
        LocalPath = currentProfile.ProfileSettings.LocalPath ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        Host = connection.Host; Port = connection.Port.ToString(); Username = connection.Username; 
    }
    
    [RelayCommand]
    private async Task ConnectToServer()
    {
        try
        {
            ServerProgressBarValue = 0;
            
            if (string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(Password) || !int.TryParse(Port, out var portNumber))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "All connection fields must be filled out correctly.").ShowAsync();
                ServerProgressBarValue = 100;
                return;
            }
            
            sftpClient = await _serverOperationService.ConnectToServer(Host,  Username, Password, int.Parse(Port));
            if (sftpClient.IsConnected)
            {
                // ServerFiles.Clear();
                var rootDir = await _serverOperationService.LoadSingleLevel(sftpClient, "/", "Root");
                ServerFiles.Add(rootDir);
            }
            
            SetBtnsVisibility();
            IsConnected = true;
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
    [RelayCommand]
    public async Task LoadDirectoryChildrenOnDemand(Directory directory)
    {
        if (directory.IsLoading || directory.ChildrenLoaded)
            return;

        directory.IsLoading = true;
        
        if (directory.FileItems.Count == 1 && directory.FileItems[0] is FileItem dummy && dummy.Path == null)
            directory.FileItems.Clear();
    
        try
        {
            var items = await _serverOperationService.LoadSingleLevel(this.sftpClient, directory.Path, directory.Name);
            // items.FileItems now contains all children with their own placeholders
            foreach (var child in items.FileItems)
                directory.FileItems.Add(child);
        
            directory.ChildrenLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading children for directory {directory.Path}: {ex}");
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to load contents of {directory.Name}").ShowAsync();
        }
        finally
        {
            directory.IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async void OpenFolder()
    {
        try
        {
            var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

            var currentProfileName = _filesAndDirectoriesService.GetCurrentProfile();
            var currentProfile = _filesAndDirectoriesService.GetUserSettings(currentProfileName);

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select file",
                AllowMultiple = true,
                SuggestedStartLocation = await window.StorageProvider.TryGetFolderFromPathAsync(currentProfile.ProfileSettings.LocalPath)
            });

            if (files.Count > 0)
            {
                LocalProgressBarValue = 50;
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
        finally
        {
            LocalProgressBarValue = 100;
        }
        
    }
    [RelayCommand]
    private async Task DeleteFileOrDirectoryFromServer()
    {
        try
        {
            var connectedMessageBox = MessageBoxManager.GetMessageBoxStandard("Warning", "Are you sure to delete this file?", ButtonEnum.YesNo);
            var result = await connectedMessageBox.ShowAsync();
            if (result == ButtonResult.Yes)
            {
                if (sftpClient.IsConnected)
                {
                    SftpFileAttributes attrs = sftpClient.GetAttributes(ServerPath);
                    if(attrs.IsDirectory)
                    {
                        await _serverOperationService.DeleteDirectoryRecursively(sftpClient, ServerPath);
                        await MessageBoxManager.GetMessageBoxStandard("Success!", "Directory have just been removed.").ShowAsync();
                    }
                    else
                    {
                        await _serverOperationService.DeleteFile(sftpClient, ServerPath);
                        await MessageBoxManager.GetMessageBoxStandard("Success!", "File have just been removed.").ShowAsync();
                    }

                    await ResetServerList();
                }
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
    private async Task UploadFileToServer()
    {
        try
        {
            if (sftpClient.IsConnected)
            {
                sftpClient.ChangeDirectory(ServerPath);
                if (LocalFiles == null || !LocalFiles.Any())
                {
                    await MessageBoxManager.GetMessageBoxStandard("Info", "No local files selected for upload.").ShowAsync();
                    return;
                }
                
                foreach (var file in LocalFiles)
                {
                    using (var fileStream = new FileStream(file.Path, FileMode.Open))
                    {
                        await _serverOperationService.UploadFile(sftpClient, fileStream, Path.GetFileName(file.Path));
                    }
                }

                await MessageBoxManager.GetMessageBoxStandard("Success!", "Files have just been uploaded to the server.").ShowAsync();;

                await ResetServerList();
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
    private async Task DownloadToLocal()
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
                string localPathForNewFile = directory[0].Path.AbsolutePath.ToString();

                if (string.IsNullOrEmpty(localPathForNewFile))
                {
                    var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Selected path is empty.");
                    await errorMessageBox.ShowAsync();
                    return;
                }
                await using (var fileStream = File.Create($"{localPathForNewFile}/{Path.GetFileName(ServerPath)}"))
                {
                    await _serverOperationService.DownloadFile(sftpClient, ServerPath, fileStream);
                }
               
                await MessageBoxManager.GetMessageBoxStandard("Success!", "Files have just been downloaded from the server.").ShowAsync();

                LocalFiles.Clear();
                LocalPath = localPathForNewFile;
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Download file error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't download the file.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task SaveConnection()
    {
        try
        {
            var connection = new Connection()
            {
                Host = Host,
                Username = Username,
                Port = int.Parse(Port)
            };

            await _connectionsRepository.SaveConnection(connection);

            var savedMessageBox = MessageBoxManager.GetMessageBoxStandard("Success.", "The connection string has been saved.");
            await savedMessageBox.ShowAsync();
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"SaveConnection error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't save the connection.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task CreateNewDirectory()
    {
        try
        {
            HomePageView.instance.NewDirectoryForm.IsVisible = false;
            if (string.IsNullOrEmpty(NewDirectoryName))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error.", "Name of the directory cannot be empty.").ShowAsync();
            }
            var path = $"{ServerPath}/{NewDirectoryName}";
            bool fileExists = await Task.Run(() => sftpClient.Exists(path));
            if (fileExists)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "A directory with this name already exists.").ShowAsync();
                return; 
            }
            await Task.Run(() => _serverOperationService.CreateDirectory(sftpClient, path));

            await ResetServerList();
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"HomePageViewModel CreateNewDirectory error: {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't create a new directory.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private void CancelNewDirectory()
    {
        HomePageView.instance.NewDirectoryForm.IsVisible = false;
    }
    [RelayCommand]
    private async Task CreateNewFile()
    {
        try
        {
            
            if (string.IsNullOrWhiteSpace(NewFileName) || !NewFileName.Contains('.'))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error.", "Name of the file cannot be empty and It must contain '.' with format of the file.").ShowAsync();
            }
            HomePageView.instance.NewFileForm.IsVisible = false;
            var path = $"{ServerPath}/{NewFileName}";

            bool fileExists = await Task.Run(() => sftpClient.Exists(path));
            if (fileExists)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "A file with this name already exists.").ShowAsync();
                return; 
            }
            await _serverOperationService.CreateFile(sftpClient, path);

            await ResetServerList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomePageViewModel CreateNewDirectory error: {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't create a new directory.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private void CancelForm()
    {
        HomePageView.instance.renameForm.IsVisible = false;
        HomePageView.instance.newDirectoryForm.IsVisible = false;
        HomePageView.instance.newFileForm.IsVisible = false;
    }
    [RelayCommand]
    private async Task Rename()
    {
        try
        {
            if (!string.IsNullOrEmpty(NewName))
            {
                SftpFileAttributes attrs = await Task.Run( () => sftpClient.GetAttributes(ServerPath));
                if (!attrs.IsDirectory && !NewName.Contains('.'))
                {
                    await MessageBoxManager.GetMessageBoxStandard("Error", "File name must include an extension.")
                        .ShowAsync();
                    return;
                }
                await _serverOperationService.Rename(sftpClient, ServerPath, NewName);
                await ResetServerList();
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("Error.", "New name is empty.").ShowAsync();
            }

            NewName = string.Empty;
            HomePageView.instance.renameForm.IsVisible = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomePageViewModel Rename error: {ex}");
            await MessageBoxManager.GetMessageBoxStandard("Error", "An unexpected error occurred while renaming.").ShowAsync();
        }
    }
    
    private void SetBtnsVisibility()
    {
        ConnectBtnVisibility = !ConnectBtnVisibility;
        DisconnectBtnVisibility = !DisconnectBtnVisibility;
    }
    internal void OpenRenameForm(Directory selectedDirectory)
    {
        HomePageView.instance.renameForm.IsVisible = true;
        if(!string.IsNullOrEmpty(selectedDirectory.Name))
            NewName = selectedDirectory.Name;
    }
    internal void OpenRenameForm(FileItem selectedFile)
    {
        HomePageView.instance.renameForm.IsVisible = true;
        if(!string.IsNullOrEmpty(selectedFile.Name))
            NewName = selectedFile.Name;
    }
    internal void OpenCreateFileForm()
    {
        HomePageView.instance.NewFileForm.IsVisible = true;
    }
    internal void OpenCreateDirectoryForm()
    {
        HomePageView.instance.NewDirectoryForm.IsVisible = true;
    }
    private async Task ResetServerList()
    {
        ServerFiles.Clear();
        ServerPath = "/";
        ServerProgressBarValue = 50;
        if (sftpClient.IsConnected)
        {
            var rootDir = await _serverOperationService.LoadSingleLevel(sftpClient, "/", "Root");
            ServerFiles.Add(rootDir);
            ServerProgressBarValue = 100;
        }
    }
}