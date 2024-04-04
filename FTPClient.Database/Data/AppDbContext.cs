using FTPClient.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace FTPClient.Database.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Connection> Connections { get; set; }
        public string DbPath = string.Empty;
        public AppDbContext()
        {
            DbPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "connections.db");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            optionsBuilder.UseSqlite($"Data Source={DbPath}" ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
        }
        
    }
}
