using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FTPClient.Database.Interfaces;
using FTPClient.Database.Repository;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using FTPClient.ViewModels;
using FTPClient.Views;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;

namespace FTPClient;

public partial class App : Application
{
    internal string currentProfileName = string.Empty;
    public IServiceProvider Services { get; private set; }
    public static App instance;
    public App()
    {
        instance = this;
    }
    public override void Initialize()
    {
        ConfigureServices();
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        IFilesAndDirectoriesService _filesAndDirectoriesService = new FilesAndDirectoriesService();
        string LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        List<Profile> allProfiles = _filesAndDirectoriesService.GetUserSettings();
        if (!allProfiles.Any())
        {
            Profile profile = new Profile()
            {
                Name = "Default",
                ProfileSettings = new ProfileSettings()
                {
                    LocalPath = LocalPath,
                    ProfileColor = new JsonColor
                    {
                        R = 36,
                        G = 39,
                        B = 42
                    },
                }
            };
            _filesAndDirectoriesService.SaveUserConfigFile(profile);
            _filesAndDirectoriesService.SaveCurrentProfile();
        }

        base.OnFrameworkInitializationCompleted();
    }
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IServerOperationService, ServerOperationService>();
        services.AddSingleton<IFilesAndDirectoriesService, FilesAndDirectoriesService>();
        services.AddScoped<IConnectionsRepository, ConnectionsRepository>();
        
        services.AddTransient<HomePageViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SettingsPageViewModel>();
        services.AddTransient<HistoryPageViewModel>();
        
        services.AddSingleton<HomePageView>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<SettingsPageView>();
        services.AddSingleton<HistoryPageView>();
        Services = services.BuildServiceProvider();
    }
}