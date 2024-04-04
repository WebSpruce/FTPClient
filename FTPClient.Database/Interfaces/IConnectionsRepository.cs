using FTPClient.Models.Models;

namespace FTPClient.Database.Interfaces
{
    public interface IConnectionsRepository
    {
        Task<List<Connection>> GetAllConnections();
        Task SaveConnection(Connection connection);
        Task DeleteConnection(Connection connection);
    }
}
