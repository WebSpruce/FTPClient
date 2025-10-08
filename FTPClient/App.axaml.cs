using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FTPClient.Database.Data;
using FTPClient.Database.Interfaces;
using FTPClient.Database.Repository;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using FTPClient.ViewModels;
using FTPClient.Views;
using HotAvalonia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FTPClient;

public partial class App : Application
{
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
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
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
        services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlite($"Data Source={Path.Join(AppDomain.CurrentDomain.BaseDirectory, "connections.db")}" ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
        });
        
        services.AddSingleton<IServerOperationService, ServerOperationService>();
        services.AddSingleton<IFilesAndDirectoriesService, FilesAndDirectoriesService>();
        services.AddScoped<IConnectionsRepository, ConnectionsRepository>();
        
        services.AddSingleton<HomePageViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<SettingsPageViewModel>();
        services.AddTransient<HistoryPageViewModel>();
        
        services.AddSingleton<HomePageView>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<SettingsPageView>();
        services.AddSingleton<HistoryPageView>();
        Services = services.BuildServiceProvider();
    }
}