using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FTPClient.Service.Interfaces;
using FTPClient.Service.Services;
using FTPClient.ViewModels;
using FTPClient.Views;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;

namespace FTPClient;

public partial class App : Application
{ 
    public IServiceProvider Services { get; private set; }
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

        base.OnFrameworkInitializationCompleted();
    }
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IServerOperationService, ServerOperationService>();
        
        services.AddTransient<HomePageViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        
        services.AddSingleton<HomePageView>();
        services.AddSingleton<MainWindow>();
        Services = services.BuildServiceProvider();
    }
}