using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FTPClient.Messages;

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

    [ObservableProperty]
    private Color _colorPickerColor = Color.FromRgb(36, 39, 42);

    private readonly IFilesAndDirectoriesService _filesAndDirectoriesService;
    private readonly IMessenger _messenger;

    public SettingsPageViewModel()  //only for design-time
    {
        CurrentProfile = "Default";
        LocalPath = "/home/user";
        Profiles = new List<Profile>
        {
            new() { Name = "Default" },
            new() { Name = "Work" }
        };
    }
    public SettingsPageViewModel(IFilesAndDirectoriesService filesAndDirectoriesService, IMessenger messenger)
    {
        _filesAndDirectoriesService = filesAndDirectoriesService;
        _messenger = messenger;
    }

    internal async Task OnLoad()
    {
        await Initialize();
    }
    
    private async Task Initialize()
    {
        var getCurrentProfileResult = _filesAndDirectoriesService.GetCurrentProfile();
        if (!getCurrentProfileResult.IsSuccess)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Get current profile error: {getCurrentProfileResult.Errors}.").ShowAsync();
            return;
        }
        CurrentProfile = getCurrentProfileResult.Value;
        
        var getUserSettingsResult = _filesAndDirectoriesService.GetUserSettings(CurrentProfile);
        if (!getUserSettingsResult.IsSuccess)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Get user settings error: {getUserSettingsResult.Errors}.").ShowAsync();
            return;
        }
        LocalPath = getUserSettingsResult.Value.ProfileSettings.LocalPath;
            
        var color = getUserSettingsResult.Value.ProfileSettings.ProfileColor;
        if (color != null)
            ColorPickerColor = Color.FromRgb(color.R, color.G, color.B);
        else
            ColorPickerColor = Color.FromRgb(36, 39, 42);
        
        await SetProfilesCombobox();
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
                LocalPath = directory[0].Path.AbsolutePath;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Open file error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't open the file.");
            await errorMessageBox.ShowAsync();
        }
    }
    private async Task SetProfilesCombobox()
    {
        var getCurrentProfileResult = _filesAndDirectoriesService.GetUserSettings();
        if (!getCurrentProfileResult.IsSuccess)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Get current profile error: {getCurrentProfileResult.Errors}.").ShowAsync();
            return;
        }
        Profiles = getCurrentProfileResult.Value;

        var currentProfile = Profiles.FirstOrDefault(p => p.Name == CurrentProfile);
        if(currentProfile == null)
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Selected profile is empty").ShowAsync();
        else
        {
            int indexOfMyProfile = Profiles.IndexOf(currentProfile);
            SelectedIndex = indexOfMyProfile;
        }
    }
    [RelayCommand]
    private async Task ChangeProfile()
    {
        await SetNewCurrentProfile(SelectedIndex);
    }
    private async Task SetNewCurrentProfile(int selectedIndex)
    {
        var profile = Profiles[selectedIndex];
        var saveCurrentProfileResult= _filesAndDirectoriesService.SaveCurrentProfile(profile.Name);
        if (!saveCurrentProfileResult.IsSuccess)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Save current profile error: {saveCurrentProfileResult.Errors}.").ShowAsync();
            return;
        }

        CurrentProfile = profile.Name;
        var getUserSettingsResult = _filesAndDirectoriesService.GetUserSettings(CurrentProfile);
        if (!getUserSettingsResult.IsSuccess)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Get user settings error: {getUserSettingsResult.Errors}.").ShowAsync();
            return;
        }
        var settings = getUserSettingsResult.Value;
        
        LocalPath = settings.ProfileSettings.LocalPath;
        var color = settings.ProfileSettings.ProfileColor;
        if(color != null)
            ColorPickerColor = Color.FromRgb(color.R, color.G, color.B);
        else
            ColorPickerColor = Color.FromRgb(36, 39, 42);
        
        _messenger.Send(new ProfileNameChangedMessage(CurrentProfile));
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
            var addNewProfileResult = _filesAndDirectoriesService.AddNewProfile(NewProfile);
            if (!addNewProfileResult.IsSuccess)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", $"Add new profile error: {addNewProfileResult.Errors}.").ShowAsync();
                return;
            }
            IsTextBoxVisible = false;
            await SetProfilesCombobox();
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
            var deleteProfileResult = _filesAndDirectoriesService.DeleteProfile(profile.Name);
            if (!deleteProfileResult.IsSuccess)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", $"Delete profile error: {deleteProfileResult.Errors}.").ShowAsync();
                return;
            }
            await SetProfilesCombobox();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SettingsPage DeleteProfile error: {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Error while removing a new profile.");
            await errorMessageBox.ShowAsync();
        }
    }
    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            _messenger.Send(new LocalPathChangedMessage(LocalPath));

            Profile profile = new Profile()
            {
                Name = CurrentProfile,
                ProfileSettings = new ProfileSettings()
                {
                    LocalPath = LocalPath,
                    ProfileColor = new JsonColor
                    {
                        R = ColorPickerColor.R, G = ColorPickerColor.G, B = ColorPickerColor.B
                    },
                }
            };
            var saveUserConfigFileResult = _filesAndDirectoriesService.SaveUserConfigFile(profile);
            if (!saveUserConfigFileResult.IsSuccess)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", $"Save user config file error: {saveUserConfigFileResult.Errors}.").ShowAsync();
                return;
            }

            var changedFolderMessageBox = MessageBoxManager.GetMessageBoxStandard("Saved", "Settings saved.");
            await changedFolderMessageBox.ShowAsync();
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"SettingsPageViewModel SaveSettings error: {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Error while saving settings.");
            await errorMessageBox.ShowAsync();
        }
    }
    internal void ColorPickerColorChanged(Color newColor)
    {
        ColorPickerColor = newColor;
        _messenger.Send(new ProfileColorChangedMessage(newColor));
    }
}