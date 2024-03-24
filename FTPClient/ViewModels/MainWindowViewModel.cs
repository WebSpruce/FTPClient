using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using FTPClient.Views;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace FTPClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isPaneOpen = true;
    
    [ObservableProperty]
    private ViewModelBase _currentPage = new HomePageViewModel();
    
    [ObservableProperty] 
    private ListItemTemplate? _selectedListItem;

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
    public MainWindowViewModel()
    {
        instance = this;
        var _filesAndDirectoriesService = ((App)Application.Current).Services.GetRequiredService<IFilesAndDirectoriesService>();
        var currentProfile = _filesAndDirectoriesService.GetCurrentProfile();
        CurrentProfileIcon = currentProfile.Substring(0,1).ToUpper();
    }
    async partial void OnSelectedListItemChanged(ListItemTemplate? item)
    {
        if (item is null)
        {
            return;
        }

        if (item.ModelType == typeof(ExitPageViewModel))
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        }

        if (CurrentPage is HomePageViewModel)
        {
            var connectedMessageBox = MessageBoxManager.GetMessageBoxStandard("Warning", "Are you sure to close this window?\nIt won't save your current connection.",ButtonEnum.YesNo);
            var result = await connectedMessageBox.ShowAsync();
            if (result == ButtonResult.Yes)
            {
                var instance = Activator.CreateInstance(item.ModelType);
                if (instance is null)
                {
                    return;
                }

                CurrentPage = (ViewModelBase)instance;
            }
            else
            {
                return;
            }
        }
        else
        {
            var instance = Activator.CreateInstance(item.ModelType);
            if (instance is null)
            {
                return;
            }

            CurrentPage = (ViewModelBase)instance;
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