using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentFTP;
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
    private ObservableCollection<Directory> _serverFiles = new();

    private FtpClient client;
    private SftpClient sftpClient;
    
    [RelayCommand]
    private async void ConnectToServer()
    {
        try
        {
            using (sftpClient = new SftpClient(new PasswordConnectionInfo(Host, Port, Username, Password)))
            {
                sftpClient.Connect();
                if (sftpClient.IsConnected)
                {
                    ServerFiles.Add(GetAllDirectories(sftpClient, "/"));
                }
            }
            
            foreach (var directory in ServerFiles)
            {
                Debug.WriteLine($"dir: {directory.Name}");
                foreach (var fileItem in directory.FileItems)
                {
                    Debug.WriteLine($"-: {fileItem.Name}");
                }
            }
            SetBtnsVisibility();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connect to the server error : {ex}");
        }
    }
    [RelayCommand]
    private void DisconnectFromServer()
    {
        try
        {
            sftpClient.Disconnect();
            SetBtnsVisibility();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connect to the server error : {ex}");
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
            foreach (var file in listOfFiles)
            {
                try
                {
                    if (file.IsDirectory && !file.Name.StartsWith(".") && !file.Name.StartsWith(".."))
                    {
                        directory.FileItems.Add(GetAllDirectories(sftpClient, file.FullName));
                    }else if (!file.IsDirectory && !file.Name.StartsWith(".") && !file.Name.StartsWith(".."))
                    {
                        directory.FileItems.Add(new FileItem { Name = file.Name, Path = file.FullName });
                    }
                }
                catch (SftpPermissionDeniedException ex)
                {
                    Debug.WriteLine($"Permission denied for directory: {file.FullName}. {ex.Message}");
                }
            }

            return directory;
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