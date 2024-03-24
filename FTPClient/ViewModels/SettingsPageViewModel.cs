using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using MsBox.Avalonia;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FTPClient.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _localPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    [ObservableProperty]
    private string _currentProfile = string.Empty;
    [ObservableProperty]
    private List<Profile> _profiles = new();
    [ObservableProperty]
    private bool _isTextBoxVisible;
    [ObservableProperty]
    private string _newProfile;

    [ObservableProperty]
    private int _selectedIndex;

    private readonly IFilesAndDirectoriesService _filesAndDirectoriesService;
    public SettingsPageViewModel()
    {
        _filesAndDirectoriesService = new FilesAndDirectoriesService();
        CurrentProfile = _filesAndDirectoriesService.GetCurrentProfile();
        LocalPath = _filesAndDirectoriesService.GetUserSettings(CurrentProfile).ProfileSettings.LocalPath;

        SetProfilesCombobox();
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

                _filesAndDirectoriesService.SaveUserConfigFile(CurrentProfile, newPath);

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
    private void SetProfilesCombobox()
    {
        Profiles = _filesAndDirectoriesService.GetUserSettings();
        int indexOfMyProfile = Profiles.IndexOf(Profiles.Where(p => p.Name == CurrentProfile).FirstOrDefault());
        SelectedIndex = indexOfMyProfile;
    }
    [RelayCommand]
    private void ChangeProfile()
    {
        SetNewCurrentProfile(SelectedIndex);
    }
    private void SetNewCurrentProfile(int selectedIndex)
    {
        var profile = Profiles[selectedIndex];
        _filesAndDirectoriesService.SaveCurrentProfile(profile.Name);

        CurrentProfile = profile.Name;
        LocalPath = _filesAndDirectoriesService.GetUserSettings(CurrentProfile).ProfileSettings.LocalPath;

        MainWindowViewModel.instance.CurrentProfileIcon = CurrentProfile.Substring(0, 1).ToUpper();
    }
    [RelayCommand]
    private void ShowAddProfileForm()
    {
        IsTextBoxVisible = true;
    }
    [RelayCommand]
    private async Task AddProfile()
    {
        try
        {
            _filesAndDirectoriesService.AddNewProfile(NewProfile);
            IsTextBoxVisible = false;
            SetProfilesCombobox();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SettingsPage AddProfile error: {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Error while adding a new profile.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task DeleteProfile()
    {
        try
        {
            var profile = Profiles[SelectedIndex];
            _filesAndDirectoriesService.DeleteProfile(profile.Name);
            SetProfilesCombobox();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SettingsPage DeleteProfile error: {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Error while removing a new profile.");
            await errorMessageBox.ShowAsync();
        }
    }
}