using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using FTPClient.Views;
using Material.Styles.Themes;
using MsBox.Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FTPClient.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _localPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private readonly IFilesAndDirectoriesService _filesAndDirectoriesService;
    public SettingsPageViewModel(IFilesAndDirectoriesService filesAndDirectoriesService)
    {
        LocalPath = HomePageViewModel.instance.LocalPath;
        _filesAndDirectoriesService = filesAndDirectoriesService;
    }
    public SettingsPageViewModel()
    {
        LocalPath = HomePageViewModel.instance.LocalPath;
        _filesAndDirectoriesService = new FilesAndDirectoriesService();
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        try
        {
            var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

            var directory = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select folder",
                AllowMultiple = false,
            });

            if (directory[0] != null)
            {
                var newPath = directory[0].Path.AbsolutePath;

                HomePageViewModel.instance.LocalPath = newPath;
                LocalPath = newPath;
                _filesAndDirectoriesService.SaveUserConfigFile(newPath);

                var changedFolderMessageBox = MessageBoxManager.GetMessageBoxStandard("Saved", "Set a new folder as default path.");
                await changedFolderMessageBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Open file error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't open the file.");
            await errorMessageBox.ShowAsync();
        }
    }
}