using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Models;
using Renci.SshNet;
using Renci.SshNet.Common;
using Directory = FTPClient.Models.Directory;

namespace FTPClient.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{
    
    private string _host;
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
    private string _username; 
    [ObservableProperty] 
    private string _password;
    [ObservableProperty] 
    private int _port;
    [ObservableProperty] 
    private bool _connectBtnVisibility = true;
    [ObservableProperty] 
    private bool _disconnectBtnVisibility;
    
    [ObservableProperty] 
    private double _serverProgressBarValue;
    [ObservableProperty] 
    private ObservableCollection<Directory> _serverFiles = new();
    [ObservableProperty] 
    private string _serverPath = "/";
    
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
    
    [RelayCommand]
    private async Task ConnectToServer()
    {
        try
        {
            ServerProgressBarValue = 0;
            sftpClient = new SftpClient(new PasswordConnectionInfo(Host, Port, Username, Password));
            await Task.Run(() =>sftpClient.Connect());
            if (sftpClient.IsConnected)
            {
                await Task.Run(() =>ServerFiles.Add(GetAllDirectories(sftpClient, "/")));
            }
            
            SetBtnsVisibility();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connect to the server error : {ex}");
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
                sftpClient.Disconnect();
                ServerFiles.Clear();
                ServerPath = "/";
            }

            SetBtnsVisibility();
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
            var listOfFiles = sftpClient.ListDirectory(path); 
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
}