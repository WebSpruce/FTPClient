using FTPClient.Database.Data;
using FTPClient.Database.Interfaces;
using FTPClient.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace FTPClient.Database.Repository
{
    public class ConnectionsRepository : IConnectionsRepository
    {
        private AppDbContext _context;
        public ConnectionsRepository()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>();
            var serviceProvider = services.BuildServiceProvider();

            _context = serviceProvider.GetService<AppDbContext>();

            _context.Database.EnsureCreatedAsync().Wait();
        }
        public async Task<List<Connection>> GetAllConnections()
        {
            return await _context.Connections.ToListAsync();
        }
        public async Task SaveConnection(Connection connection)
        {
            try
            {
                await _context.Connections.AddAsync(connection);
                await _context.SaveChangesAsync();
            }catch(Exception ex)
            {
                Debug.WriteLine($"ConnectionRepository SaveConnection error: {ex}");
            }
        }
        public async Task DeleteConnection(Connection connection)
        {
            try
            {
                var item = _context.Connections.Where(c=>c.Id == connection.Id).FirstOrDefault();
                _context.Connections.Remove(item);
                await _context.SaveChangesAsync();
            }catch(Exception ex)
            {
                Debug.WriteLine($"ConnectionRepository DeleteConnection error: {ex}");
            }
        }
    }
}
