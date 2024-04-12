using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Helper;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using FTPClient.Views;
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

    [ObservableProperty]
    private Color _colorPickerColor = Color.FromRgb(36, 39, 42);

    private readonly IFilesAndDirectoriesService _filesAndDirectoriesService;
    private string newPath = string.Empty;
    public static SettingsPageViewModel instance;
    public SettingsPageViewModel()
    {
        instance = this;
        _filesAndDirectoriesService = new FilesAndDirectoriesService();
        CurrentProfile = _filesAndDirectoriesService.GetCurrentProfile();
        LocalPath = _filesAndDirectoriesService.GetUserSettings(CurrentProfile).ProfileSettings.LocalPath;

        var color = _filesAndDirectoriesService.GetUserSettings(CurrentProfile).ProfileSettings.ProfileColor;
        if (color != null)
        {
            ColorPickerColor = Color.FromRgb(color.R, color.G, color.B);
        }
        else
        {
            ColorPickerColor = Color.FromRgb(36, 39, 42);
        }
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
        var settings = _filesAndDirectoriesService.GetUserSettings(CurrentProfile);
        LocalPath = settings.ProfileSettings.LocalPath;
        var color = settings.ProfileSettings.ProfileColor;
        if(color != null)
        {
            ColorPickerColor = Color.FromRgb(color.R, color.G, color.B);
        }
        else
        {
            ColorPickerColor = Color.FromRgb(36, 39, 42);
        }

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
    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            HomePageViewModel.instance.LocalPath = newPath;

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
            _filesAndDirectoriesService.SaveUserConfigFile(profile);

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
        if (DarkOrLightColor.IsLightColor(ColorPickerColor))
        {
            MainWindow.instance.ProfileIcon.Foreground = new SolidColorBrush(Color.FromRgb(36, 39, 42));
        }
        else
        {
            MainWindow.instance.ProfileIcon.Foreground = new SolidColorBrush(Color.FromRgb(107, 139, 161));
        }
        MainWindow.instance.ProfileIcon.Background = new SolidColorBrush(ColorPickerColor);
    }
}