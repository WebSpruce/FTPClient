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
using FTPClient.Session;

namespace FTPClient.ViewModels;

public partial class HistoryPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage = new HomePageViewModel();
    [ObservableProperty]
    private List<Connection> _connections = new();
    private IConnectionsRepository _connectionRepository;
    public static HistoryPageViewModel instance;
    public HistoryPageViewModel(IConnectionsRepository connectionsRepository)
    {
        instance = this;
        _connectionRepository = connectionsRepository;
        Task.Run(async () => Connections = await _connectionRepository.GetAllConnections());
    } 
    internal async Task OnLoad()
    {
        Connections = await _connectionRepository.GetAllConnections();
    }
    public async Task Connect(Connection connection)
    {
        try
        {
            MainWindowViewModel.instance.SelectedListItemMain = new ListItemTemplate(typeof(HomePageViewModel), "HomeRegular");
            MainWindowViewModel.instance.CurrentPage = new HomePageViewModel(connection);
            SessionConnection.Instance.ClearSession();
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
            Connections = _connectionRepository.GetAllConnections().Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HistoryPageViewModel Remove error : {ex}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't remove the connection string.");
            await errorMessageBox.ShowAsync();
        }
    }

}