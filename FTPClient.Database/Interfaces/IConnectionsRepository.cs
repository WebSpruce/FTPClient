using FTPClient.Models.Models;

namespace FTPClient.Database.Interfaces
{
    public interface IConnectionsRepository
    {
        Task<List<Connection>> GetAllConnections(CancellationToken token);
        Task SaveConnection(Connection connection, CancellationToken token);
        Task DeleteConnection(Connection connection, CancellationToken token);
    }
}
