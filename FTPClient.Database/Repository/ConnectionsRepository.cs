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
        public async Task<List<Connection>> GetAllConnections(CancellationToken token)
        {
            await using var context = await _context.CreateDbContextAsync(token);
            return await context.Connections.ToListAsync(token);
        }
        public async Task SaveConnection(Connection connection, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                    return;
                await using var context = await _context.CreateDbContextAsync(token);
                await context.Connections.AddAsync(connection, token);
                await context.SaveChangesAsync(token);
            }catch(Exception ex)
            {
                Debug.WriteLine($"ConnectionRepository SaveConnection error: {ex}");
            }
        }
        public async Task DeleteConnection(Connection connection, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                    return;
                await using var context = await _context.CreateDbContextAsync(token);
                var item = await context.Connections.FirstOrDefaultAsync(c=>c.Id == connection.Id, token);
                if (item is not null)
                {
                    context.Connections.Remove(item);
                    await context.SaveChangesAsync(token);
                }
            }catch(Exception ex)
            {
                Debug.WriteLine($"ConnectionRepository DeleteConnection error: {ex}");
            }
        }
    }
}
