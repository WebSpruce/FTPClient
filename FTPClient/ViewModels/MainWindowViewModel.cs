using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FTPClient.Helper;
using FTPClient.Messages;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Session;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;

namespace FTPClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, 
    IRecipient<NavigateToHomeMessage>,
    IRecipient<ProfileColorChangedMessage>,
    IRecipient<ProfileNameChangedMessage>
{
    [ObservableProperty]
    private bool _isPaneOpen = true;
    
    [ObservableProperty]
    private ViewModelBase _currentPage = default!;
    
    [ObservableProperty] private ListItemTemplate? _selectedListItemMain;
    [ObservableProperty] private ListItemTemplate? _selectedListItemFooter;
    [ObservableProperty] private IBrush? _profileIconForeground = new SolidColorBrush(Color.FromRgb(153,170,181));
    [ObservableProperty] private IBrush? _profileIconBackground = new SolidColorBrush(Colors.Transparent);

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
    internal Dictionary<Type, ViewModelBase> pagesDictionary = new Dictionary<Type, ViewModelBase>();
    private readonly IServiceProvider _serviceProvider;
    private readonly IHomePageViewModelFactory _homeFactory;
    private readonly IMessenger _messenger;
    private readonly ISessionConnection _sessionConnection;
    private readonly IFilesAndDirectoriesService _filesAndDirectoriesService;
    public MainWindowViewModel(
        IServiceProvider serviceProvider, 
        IMessenger messenger, 
        ISessionConnection sessionConnection, 
        IFilesAndDirectoriesService filesAndDirectoriesService,
        IHomePageViewModelFactory homeFactory)
    {
        _serviceProvider = serviceProvider;
        _messenger = messenger;
        _sessionConnection = sessionConnection;
        _filesAndDirectoriesService = filesAndDirectoriesService;
        _homeFactory = homeFactory;
        _messenger.RegisterAll(this);
        
        _currentPage = _homeFactory.Create();

        Task.Run(async () => await Initialize());
    }

    private async Task Initialize()
    {
        var getCurrentProfileResult = _filesAndDirectoriesService.GetCurrentProfile();
        if (!getCurrentProfileResult.IsSuccess)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", $"Get current user profile error: {getCurrentProfileResult.Errors}.").ShowAsync();
            return;
        }

        var currentProfile = getCurrentProfileResult.Value;
        
        Color profileColor = Color.FromRgb(36, 39, 42);
        JsonColor profileJsonColor = new JsonColor() { R =  profileColor.R, G = profileColor.G, B = profileColor.B };
        CurrentProfileIcon = "D";
        if (!string.IsNullOrEmpty(currentProfile))
        {
            CurrentProfileIcon = currentProfile.Substring(0,1).ToUpper();
            var getUserSettingsResult = _filesAndDirectoriesService.GetUserSettings(currentProfile);
            if (!getUserSettingsResult.IsSuccess)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", $"Get user settings error: {getUserSettingsResult.Errors}.").ShowAsync();
                return;
            }

            var userSettings = getUserSettingsResult.Value;
                
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
        ProfileIconForeground = new ImmutableSolidColorBrush(foregroundColor);
        ProfileIconBackground = new ImmutableSolidColorBrush(profileColor);
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
                _sessionConnection.ClearSession();
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


    public void Receive(NavigateToHomeMessage message)
    {
        var homeVm = _homeFactory.Create(message.Value);

        // Update sidebar selection
        SelectedListItemMain = MainMenuItems
            .First(x => x.ModelType == typeof(HomePageViewModel));

        // Clear footer selection to avoid dual highlight
        _selectedListItemFooter = null;
        OnPropertyChanged(nameof(SelectedListItemFooter));

        CurrentPage = homeVm;
        // Refresh page cache so next navigation uses the new instance
        pagesDictionary[typeof(HomePageViewModel)] = homeVm;
    }

    public void Receive(ProfileColorChangedMessage message)
    {
        var newColor = message.Value;

        Color foregroundColor = DarkOrLightColor.IsLightColor(newColor)
            ? Color.FromRgb(36, 39, 42)
            : Color.FromRgb(107, 139, 161);

        ProfileIconForeground = new ImmutableSolidColorBrush(foregroundColor);
        ProfileIconBackground = new ImmutableSolidColorBrush(newColor);
    }

    public void Receive(ProfileNameChangedMessage message)
    {
        var profileName = message.Value;
        CurrentProfileIcon = string.IsNullOrWhiteSpace(profileName)
            ? "D"
            : profileName.Substring(0, 1).ToUpper();
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