using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FTPClient.Database.Interfaces;
using FTPClient.Database.Repository;
using FTPClient.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FTPClient.ViewModels;

public partial class HistoryPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private List<Connection> _connections = new();
    private IConnectionsRepository _connectionRepository;
    public static HistoryPageViewModel instance;
    public HistoryPageViewModel(IConnectionsRepository connectionsRepository)
    {
        instance = this;
        _connectionRepository = connectionsRepository;
        Connections = _connectionRepository.GetAllConnections().Result;
    } 
    public HistoryPageViewModel()
    {
        instance = this;
        var services = new ServiceCollection();
        services.AddScoped<IConnectionsRepository, ConnectionsRepository>();
        var serviceProvider = services.BuildServiceProvider();
        _connectionRepository = serviceProvider.GetService<IConnectionsRepository>();

        LoadConnections();
    }
    private async Task LoadConnections()
    {
        Connections = await _connectionRepository.GetAllConnections();
    }
    [ObservableProperty]
    private ViewModelBase _currentPage = new HomePageViewModel();
    public async Task Connect(Connection connection)
    {
        try
        {
            MainWindowViewModel.instance.CurrentPage = new HomePageViewModel(connection);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HistoryPageViewModel connect error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't add the connection string.");
            await errorMessageBox.ShowAsync();
        }
    } 
    public async Task Remove(Connection connection)
    {
        try
        {
            await _connectionRepository.DeleteConnection(connection);
            var savedMessageBox = MessageBoxManager.GetMessageBoxStandard("Success.", "The connection string has been deleted.");
            await savedMessageBox.ShowAsync();
            await LoadConnections();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HistoryPageViewModel Remove error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't remove the connection string.");
            await errorMessageBox.ShowAsync();
        }
    }

}