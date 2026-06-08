namespace FTPClient.Service.Abstractions;

public interface IRemoteSession : IAsyncDisposable
{
    bool IsConnected { get; }
}