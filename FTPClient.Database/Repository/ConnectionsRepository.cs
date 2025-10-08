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
        private readonly IDbContextFactory<AppDbContext> _context;
        public ConnectionsRepository(IDbContextFactory<AppDbContext> context)
        {
            _context = context;
        }
        public async Task<List<Connection>> GetAllConnections()
        {
            await using var context = await _context.CreateDbContextAsync();
            return await context.Connections.ToListAsync();
        }
        public async Task SaveConnection(Connection connection)
        {
            try
            {
                await using var context = await _context.CreateDbContextAsync();
                await context.Connections.AddAsync(connection);
                await context.SaveChangesAsync();
            }catch(Exception ex)
            {
                Debug.WriteLine($"ConnectionRepository SaveConnection error: {ex}");
            }
        }
        public async Task DeleteConnection(Connection connection)
        {
            try
            {
                await using var context = await _context.CreateDbContextAsync();
                var item = context.Connections.FirstOrDefault(c=>c.Id == connection.Id);
                if (item is not null)
                {
                    context.Connections.Remove(item);
                    await context.SaveChangesAsync();
                }
            }catch(Exception ex)
            {
                Debug.WriteLine($"ConnectionRepository DeleteConnection error: {ex}");
            }
        }
    }
}
