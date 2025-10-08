using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Helper;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Session;
using FTPClient.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FTPClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isPaneOpen = true;
    
    [ObservableProperty]
    private ViewModelBase _currentPage = new HomePageViewModel();
    
    [ObservableProperty] private ListItemTemplate? _selectedListItemMain;
    [ObservableProperty] private ListItemTemplate? _selectedListItemFooter;
    [ObservableProperty] private SolidColorBrush? _profileIconForeground = new SolidColorBrush(Color.FromRgb(153,170,181));
    [ObservableProperty] private SolidColorBrush? _profileIconBackground = new SolidColorBrush(Colors.Transparent);

    [ObservableProperty] 
    private string? _currentProfileIcon;

    public ObservableCollection<ListItemTemplate> MainMenuItems { get; } = new()
    {
        new ListItemTemplate(typeof(HomePageViewModel), "HomeRegular"),
        new ListItemTemplate(typeof(HistoryPageViewModel), "HistoryRegular"),
    };
    public ObservableCollection<ListItemTemplate> FooterMenuItems { get; } = new()
    {
        new ListItemTemplate(typeof(SettingsPageViewModel), "SettingsRegular"),
        new ListItemTemplate(typeof(ExitPageViewModel), "ExitRegular"),
    };
    public static MainWindowViewModel instance;
    internal Dictionary<Type, ViewModelBase> pagesDictionary = new Dictionary<Type, ViewModelBase>();
    private readonly IServiceProvider _serviceProvider;
    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        instance = this;

        _serviceProvider = serviceProvider;
        var _filesAndDirectoriesService = ((App)Application.Current).Services.GetRequiredService<IFilesAndDirectoriesService>();
        var currentProfile = _filesAndDirectoriesService.GetCurrentProfile();

        Color profileColor = Color.FromRgb(36, 39, 42);
        JsonColor profileJsonColor = new JsonColor() { R =  profileColor.R, G = profileColor.G, B = profileColor.B };
        CurrentProfileIcon = "D";
        if (!string.IsNullOrEmpty(currentProfile))
        {
            CurrentProfileIcon = currentProfile.Substring(0,1).ToUpper();
            var userSettings = _filesAndDirectoriesService.GetUserSettings(currentProfile);

            if(userSettings.ProfileSettings == null)
            {
                userSettings.ProfileSettings = new ProfileSettings() { ProfileColor = profileJsonColor, LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) };
            }
            var userColorSettings = userSettings.ProfileSettings.ProfileColor;
            if (userColorSettings != null)
            {
                profileColor = Color.FromRgb(userColorSettings.R, userColorSettings.G, userColorSettings.B);
            }
        }

        Color foregroundColor = DarkOrLightColor.IsLightColor(profileColor) ? Color.FromRgb(36, 39, 42) : Color.FromRgb(107, 139, 161);
        ProfileIconForeground = new SolidColorBrush(foregroundColor);
        ProfileIconBackground = new SolidColorBrush(profileColor);
    }

    partial void OnSelectedListItemMainChanged(ListItemTemplate? item)
    {
        if(item is null) return;

        _selectedListItemFooter = null; //clear the footer list selection to prevent dual selection
        OnPropertyChanged(nameof(SelectedListItemFooter));
        NavigateToSelectedPage(item);
    }
    partial void OnSelectedListItemFooterChanged(ListItemTemplate? item)
    {
        if(item is null) return;

        _selectedListItemMain = null; //clear the main list selection to prevent dual selection
        OnPropertyChanged(nameof(SelectedListItemMain));
        NavigateToSelectedPage(item);
    }
    private void NavigateToSelectedPage(ListItemTemplate? item)
    {
        if (item is null) return;

        if (item.ModelType == typeof(ExitPageViewModel))
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                SessionConnection.Instance.ClearSession();
                desktopApp.Shutdown();
            }
            return;
        }

        if (!pagesDictionary.ContainsKey(CurrentPage.GetType()))
        {
            var page = (ViewModelBase)CurrentPage;
            pagesDictionary.Add(CurrentPage.GetType(), page);
        }

        var modelType = item.ModelType;
        if (pagesDictionary.ContainsKey(modelType))
        {
            CurrentPage = pagesDictionary[modelType];
        }
        else
        {
            var instance = _serviceProvider.GetRequiredService(modelType);
            if (instance is null)
            {
                return;
            }

            CurrentPage = (ViewModelBase)instance;
            pagesDictionary.Add(modelType, CurrentPage);
        }
    }
    
    [RelayCommand]
    private void SplitViewTrigger()
    {
        IsPaneOpen = !IsPaneOpen;
    }
    
    
}

public class ListItemTemplate
{
    public string Label { get; }
    public Type ModelType { get; }
    public StreamGeometry ListItemIcon { get; }
    public ListItemTemplate(Type type, string iconKey)
    {
        ModelType = type;
        Label = type.Name.Replace("PageViewModel", "");
        Application.Current!.TryFindResource(iconKey, out var res);
        ListItemIcon = (StreamGeometry)res;
    }
}