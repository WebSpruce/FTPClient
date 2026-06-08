using FTPClient.Database.Interfaces;
using FTPClient.Models.Models;
using FTPClient.Service.Interfaces;
using FTPClient.Session;

namespace FTPClient.ViewModels;

public sealed class HomePageViewModelFactory : IHomePageViewModelFactory
{
    private readonly IServerOperationService _serverOperationService;
    private readonly IFilesAndDirectoriesService _filesAndDirectoriesService;
    private readonly IConnectionsRepository _connectionsRepository;
    private readonly ISessionConnection _sessionConnection;

    public HomePageViewModelFactory(
        IServerOperationService serverOperationService,
        IFilesAndDirectoriesService filesAndDirectoriesService,
        IConnectionsRepository connectionsRepository,
        ISessionConnection sessionConnection)
    {
        _serverOperationService = serverOperationService;
        _filesAndDirectoriesService = filesAndDirectoriesService;
        _connectionsRepository = connectionsRepository;
        _sessionConnection = sessionConnection;
    }

    public HomePageViewModel Create(Connection? connection = null)
    {
        return new HomePageViewModel(
            _serverOperationService,
            _filesAndDirectoriesService,
            _connectionsRepository,
            _sessionConnection,
            connection);
    }
}