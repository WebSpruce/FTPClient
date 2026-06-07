using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Database.Interfaces;
using FTPClient.Models;
using FTPClient.Models.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Session;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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
    
    [ObservableProperty]
    private bool _isCreateDirectoryFormVisible = false;
    [ObservableProperty]
    private bool _isCreateFileFormVisible = false;
    [ObservableProperty]
    private bool _isRenameFormVisible = false;
    
    private IServerOperationService _serverOperationService;
    private IFilesAndDirectoriesService _filesAndDirectoriesService;
    private IConnectionsRepository _connectionsRepository;
    private ISessionConnection _sessionConnection;
    private CancellationTokenSource _ctsSource;
    private CancellationToken _cts;
    public static HomePageViewModel instance;
    public ICommand DirectoryExpandedCommand { get; set; }
    public HomePageViewModel() { }
    public HomePageViewModel(IServerOperationService serverOperationService,
        IFilesAndDirectoriesService filesAndDirectoriesService,
        IConnectionsRepository connectionsRepository,
        ISessionConnection sessionConnection,
        Connection? connection = null)
    {
        Initialize(serverOperationService, filesAndDirectoriesService, connectionsRepository, sessionConnection);
        if (connection is not null)
        {
            Host = connection.Host;
            Port = connection.Port.ToString();
            Username = connection.Username;
        }
    }

    private void Initialize(IServerOperationService serverOperationService,
        IFilesAndDirectoriesService filesAndDirectoriesService,
        IConnectionsRepository connectionsRepository,
        ISessionConnection sessionConnection)
    {
        _sessionConnection = sessionConnection;
        _serverOperationService = serverOperationService;
        _filesAndDirectoriesService = filesAndDirectoriesService;
        _connectionsRepository = connectionsRepository;

        _ctsSource = new CancellationTokenSource();
        _cts = _ctsSource.Token;
        
        DirectoryExpandedCommand = new AsyncRelayCommand<Directory>(LoadDirectoryChildrenOnDemand);
    }

    internal async Task OnLoad()
    {
        var profileName = _filesAndDirectoriesService.GetCurrentProfile();
        var currentProfile = _filesAndDirectoriesService.GetUserSettings(profileName);
        LocalPath = currentProfile.ProfileSettings.LocalPath ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (_sessionConnection.CurrentConnection != null)
        {
            Host = _sessionConnection.CurrentConnection.Host;
            Port = _sessionConnection.CurrentConnection.Port.ToString();
            Username = _sessionConnection.CurrentConnection.Username;
            
            if (_sessionConnection.CurrentSftpClient is not null)
            {
                ConnectBtnVisibility = false;
                DisconnectBtnVisibility = true;
                ServerFiles.Clear();
                ServerPath = "/";
                
                var rootDirResult = await _serverOperationService.LoadSingleLevel(_sessionConnection.CurrentSftpClient, "/", _cts, "Root");
                if (!rootDirResult.IsSuccess)
                {
                    var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", $"Loading data error: {rootDirResult.Errors}.");
                    await errorMessageBox.ShowAsync();
                }
                ServerFiles.Add(rootDirResult.Value);
            }
        }
    }
    
    [RelayCommand]
    private async Task ConnectToServer(CancellationToken token)
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
            
            var connectResult = await _serverOperationService.ConnectToServer(Host,  Username, Password, int.Parse(Port), token);
            if (!connectResult.IsSuccess)
            {
                var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", $"Connecting error: {connectResult.Errors}.");
                await errorMessageBox.ShowAsync();
            }

            _sessionConnection.CurrentSftpClient = connectResult.Value;
            if (_sessionConnection.IsConnected)
            {
                _sessionConnection.CurrentConnection = new Connection()
                {
                    Host = Host,
                    Username = Username,
                    Port = int.Parse(Port),
                    Id = 0
                };
                ServerFiles.Clear();
                
                var rootDirResult = await _serverOperationService.LoadSingleLevel(_sessionConnection.CurrentSftpClient, "/", token, "Root");
                if (!connectResult.IsSuccess)
                {
                    var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", $"Loading error: {rootDirResult.Errors}.");
                    await errorMessageBox.ShowAsync();
                }
                ServerFiles.Add(rootDirResult.Value);
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
            Debug.WriteLine($"Connect to the server error : {ex.Message} - {ex.InnerException}");
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
            if (_sessionConnection.IsConnected)
            {
                _serverOperationService.Disconnect(_sessionConnection.CurrentSftpClient);
                if (!_sessionConnection.IsConnected)
                {
                    ServerFiles.Clear();
                    ServerPath = "/";
                    _sessionConnection.ClearSession();
                }
            }
            SetBtnsVisibility();
            IsConnected = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Disconnect from the server error : {ex.Message} - {ex.InnerException}");
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
            var loadSingleLevelResult = await _serverOperationService.LoadSingleLevel(_sessionConnection.CurrentSftpClient, directory.Path, _cts, directory.Name);
            if(!loadSingleLevelResult.IsSuccess)
                await MessageBoxManager.GetMessageBoxStandard("Error.", $"Load single level error: {loadSingleLevelResult.Errors}").ShowAsync();
            var items = loadSingleLevelResult.Value;
            // items.FileItems now contains all children with their own placeholders
            foreach (var child in items.FileItems)
                directory.FileItems.Add(child);
        
            directory.ChildrenLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading children for directory {directory.Path}: {ex.Message} - {ex.InnerException}");
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
            Debug.WriteLine($"Open file error : {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't open the file.");
            await errorMessageBox.ShowAsync();
        }
        finally
        {
            LocalProgressBarValue = 100;
        }
        
    }
    [RelayCommand]
    private async Task DeleteFileOrDirectoryFromServer(CancellationToken token)
    {
        try
        {
            var connectedMessageBox = MessageBoxManager.GetMessageBoxStandard("Warning", "Are you sure to delete this file?", ButtonEnum.YesNo);
            var result = await connectedMessageBox.ShowAsync();
            if (result == ButtonResult.Yes)
            {
                if (_sessionConnection.IsConnected)
                {
                     var getAttributesResult = _serverOperationService.GetAttributes(_sessionConnection.CurrentSftpClient, ServerPath);
                     if (!getAttributesResult.IsSuccess)
                         await MessageBoxManager.GetMessageBoxStandard("Error.", $"Get attributes error: {getAttributesResult.Errors}").ShowAsync();

                    SftpFileAttributes attrs = getAttributesResult.Value;
                    if (attrs is null)
                        return;
                    if(attrs.IsDirectory)
                    {
                        await _serverOperationService.DeleteDirectoryRecursively(_sessionConnection.CurrentSftpClient, ServerPath, token);
                        await MessageBoxManager.GetMessageBoxStandard("Success!", "Directory have just been removed.").ShowAsync();
                    }
                    else
                    {
                        await _serverOperationService.DeleteFile(_sessionConnection.CurrentSftpClient, ServerPath, token);
                        await MessageBoxManager.GetMessageBoxStandard("Success!", "File have just been removed.").ShowAsync();
                    }

                    await ResetServerList(token);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Remove file error : {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't delete the file.");
            await errorMessageBox.ShowAsync();
        }
    } 
    [RelayCommand]
    private async Task UploadFileToServer(CancellationToken token)
    {
        try
        {
            if (_sessionConnection.IsConnected)
            {
                _serverOperationService.ChangeDirectory(_sessionConnection.CurrentSftpClient, ServerPath);
                if (LocalFiles == null || !LocalFiles.Any())
                {
                    await MessageBoxManager.GetMessageBoxStandard("Info", "No local files selected for upload.").ShowAsync();
                    return;
                }
                
                foreach (var file in LocalFiles)
                {
                    using (var fileStream = new FileStream(file.Path, FileMode.Open))
                    {
                        await _serverOperationService.UploadFile(_sessionConnection.CurrentSftpClient, fileStream, Path.GetFileName(file.Path), token);
                    }
                }

                await MessageBoxManager.GetMessageBoxStandard("Success!", "Files have just been uploaded to the server.").ShowAsync();;

                await ResetServerList(token);
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Upload file error : {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't upload the file.");
            await errorMessageBox.ShowAsync();
        }
    } 
    [RelayCommand]
    private async Task DownloadToLocal(CancellationToken token)
    {
        try
        {
            if (_sessionConnection.IsConnected)
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
                    await _serverOperationService.DownloadFile(_sessionConnection.CurrentSftpClient, ServerPath, fileStream, token);
                }
               
                await MessageBoxManager.GetMessageBoxStandard("Success!", "Files have just been downloaded from the server.").ShowAsync();

                LocalFiles.Clear();
                LocalPath = localPathForNewFile;
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Download file error : {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't download the file.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task SaveConnection(CancellationToken token)
    {
        try
        {
            var connection = new Connection()
            {
                Host = Host,
                Username = Username,
                Port = int.Parse(Port)
            };

            await _connectionsRepository.SaveConnection(connection, token);

            var savedMessageBox = MessageBoxManager.GetMessageBoxStandard("Success.", "The connection string has been saved.");
            await savedMessageBox.ShowAsync();
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"SaveConnection error : {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't save the connection.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task CreateNewDirectory(CancellationToken token)
    {
        try
        {
            CloseAllForms();
            if (string.IsNullOrEmpty(NewDirectoryName))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error.", "Name of the directory cannot be empty.").ShowAsync();
            }
            var path = $"{ServerPath}/{NewDirectoryName}";
            var isPathExistsResult = _serverOperationService.IsPathExists(_sessionConnection.CurrentSftpClient, path);
            if(!isPathExistsResult.IsSuccess)
                await MessageBoxManager.GetMessageBoxStandard("Error.", $"Is path exists error: {isPathExistsResult.Errors}").ShowAsync();
            bool fileExists = isPathExistsResult.Value; 
            if (fileExists)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "A directory with this name already exists.").ShowAsync();
                return; 
            }
            await Task.Run(() => _serverOperationService.CreateDirectory(_sessionConnection.CurrentSftpClient, path, token));

            await ResetServerList(token);
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"HomePageViewModel CreateNewDirectory error: {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't create a new directory.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task CreateNewFile(CancellationToken token)
    {
        try
        {
            
            if (string.IsNullOrWhiteSpace(NewFileName) || !NewFileName.Contains('.'))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error.", "Name of the file cannot be empty and It must contain '.' with format of the file.").ShowAsync();
            }
            CloseAllForms();
            var path = $"{ServerPath}/{NewFileName}";

            var isPathExistsResult = _serverOperationService.IsPathExists(_sessionConnection.CurrentSftpClient, path);
            if(!isPathExistsResult.IsSuccess)
                await MessageBoxManager.GetMessageBoxStandard("Error.", $"Is path exists error: {isPathExistsResult.Errors}").ShowAsync();
            bool fileExists = isPathExistsResult.Value;
            if (fileExists)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "A file with this name already exists.").ShowAsync();
                return; 
            }
            await _serverOperationService.CreateFile(_sessionConnection.CurrentSftpClient, path, token);

            await ResetServerList(token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomePageViewModel CreateNewDirectory error: {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't create a new directory.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task Rename(CancellationToken token)
    {
        try
        {
            if (!string.IsNullOrEmpty(NewName))
            {
                var getAttributes = _serverOperationService.GetAttributes(_sessionConnection.CurrentSftpClient, ServerPath);
                if(!getAttributes.IsSuccess)
                    await MessageBoxManager.GetMessageBoxStandard("Error.", $"Rename error: {getAttributes.Errors}").ShowAsync();
                var attrs = getAttributes.Value;
                if (attrs is null)
                    return;
                if (!attrs.IsDirectory && !NewName.Contains('.'))
                {
                    await MessageBoxManager.GetMessageBoxStandard("Error", "File name must include an extension.")
                        .ShowAsync();
                    return;
                }
                await _serverOperationService.Rename(_sessionConnection.CurrentSftpClient, ServerPath, NewName, token);
                await ResetServerList(token);
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("Error.", "New name is empty.").ShowAsync();
            }

            NewName = string.Empty;
            CloseAllForms();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomePageViewModel Rename error: {ex.Message} - {ex.InnerException}");
            await MessageBoxManager.GetMessageBoxStandard("Error", "An unexpected error occurred while renaming.").ShowAsync();
        }
    }
    
    private CancellationToken RefreshToken()
    {
        _ctsSource.Cancel();
        _ctsSource.Dispose();
        _ctsSource = new CancellationTokenSource();
        return _ctsSource.Token;
    }
    
    [RelayCommand]
    private async Task DirectoryLeftClicked(Directory directory)
    {
        if (directory is null) return;
        var token = RefreshToken();
        SelectedServerItem = directory;
        await LoadDirectoryChildrenOnDemand(directory);
    }

    [RelayCommand]
    private void DirectoryRightClicked(Directory directory)
    {
        if (directory is null) return;
        // Store the selected item so context menu actions know what to operate on
        SelectedServerItem = directory;
    }

    [RelayCommand]
    private void FileRightClicked(FileItem file)
    {
        if (file is null) return;
        SelectedFileItem = file;
    }
    
    [RelayCommand]
    internal void CloseAllForms()
    {
        IsCreateDirectoryFormVisible = false;
        IsCreateFileFormVisible = false;
        IsRenameFormVisible = false;
    }
    
    private void SetBtnsVisibility()
    {
        ConnectBtnVisibility = !ConnectBtnVisibility;
        DisconnectBtnVisibility = !DisconnectBtnVisibility;
    }
    internal void OpenRenameForm(object item)
    {
        IsRenameFormVisible = true;
        IsCreateDirectoryFormVisible = false;
        IsCreateFileFormVisible = false;

        NewName = item switch
        {
            Directory dir => dir.Name,
            FileItem file => file.Name,
            _ => string.Empty
        };
    }
    internal void OpenCreateFileForm()
    {
        IsCreateFileFormVisible = true;
        IsCreateDirectoryFormVisible = false;
        IsRenameFormVisible = false;
    }
    internal void OpenCreateDirectoryForm()
    {
        IsCreateDirectoryFormVisible = true;
        IsCreateFileFormVisible = false;
        IsRenameFormVisible = false;
    }

    private async Task ResetServerList(CancellationToken token)
    {
        ServerFiles.Clear();
        ServerPath = "/";
        ServerProgressBarValue = 50;
        if (_sessionConnection.IsConnected)
        {
            var loadSingleLevelResult = await _serverOperationService.LoadSingleLevel(_sessionConnection.CurrentSftpClient, "/", token, "Root");
            if(!loadSingleLevelResult.IsSuccess)
                await MessageBoxManager.GetMessageBoxStandard("Error.", $"Load single level error: {loadSingleLevelResult.Errors}").ShowAsync();
            
            var rootDir = loadSingleLevelResult.Value;
            ServerFiles.Add(rootDir);
            ServerProgressBarValue = 100;
        }
    }
}