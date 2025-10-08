using FTPClient.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace FTPClient.Database.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Connection> Connections { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> optionsBuilder) : base(optionsBuilder)
        {
        }
    }
}
