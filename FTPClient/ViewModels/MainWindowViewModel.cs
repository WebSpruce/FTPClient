﻿using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FTPClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isPaneOpen = true;
    
    [ObservableProperty] 
    private ViewModelBase _currentPage = new HomePageViewModel();
    
    [ObservableProperty] 
    private ListItemTemplate? _selectedListItem;
    
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

    partial void OnSelectedListItemChanged(ListItemTemplate? item)
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
        var instance = Activator.CreateInstance(item.ModelType);
        if (instance is null)
        {
            return;
        }

        CurrentPage = (ViewModelBase)instance;
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