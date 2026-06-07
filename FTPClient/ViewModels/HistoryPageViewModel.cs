using CommunityToolkit.Mvvm.ComponentModel;
using FTPClient.Database.Interfaces;
using FTPClient.Models.Models;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FTPClient.Messages;

namespace FTPClient.ViewModels;

public partial class HistoryPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage = default!;
    [ObservableProperty]
    private List<Connection> _connections = new();
    private IConnectionsRepository _connectionRepository;
    private readonly IMessenger _messenger;
    private readonly CancellationTokenSource _ctsSource;
    private readonly CancellationToken _cts;
    public HistoryPageViewModel(IConnectionsRepository connectionsRepository, IMessenger messenger)
    {
        _connectionRepository = connectionsRepository;
        _messenger = messenger;
        _ctsSource = new CancellationTokenSource();
        _cts = _ctsSource.Token;
        Task.Run(async () => Connections = await _connectionRepository.GetAllConnections(_cts));
    } 
    internal async Task OnLoad()
    {
        Connections = await _connectionRepository.GetAllConnections(_cts);
    }
    [RelayCommand]
    private async Task Connect(Connection connection)
    {
        try
        {
            _messenger.Send(new NavigateToHomeMessage(connection));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HistoryPageViewModel connect error : {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't add the connection string.");
            await errorMessageBox.ShowAsync();
        }
    } 
    [RelayCommand]
    public async Task Remove(Connection connection, CancellationToken token)
    {
        try
        {
            await _connectionRepository.DeleteConnection(connection, token);
            var savedMessageBox = MessageBoxManager.GetMessageBoxStandard("Success.", "The connection string has been deleted.");
            await savedMessageBox.ShowAsync();
            Connections = _connectionRepository.GetAllConnections(token).Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HistoryPageViewModel Remove error : {ex.Message} - {ex.InnerException}");
            var errorMessageBox = MessageBoxManager.GetMessageBoxStandard("Error.", "Couldn't remove the connection string.");
            await errorMessageBox.ShowAsync();
        }
    }

}