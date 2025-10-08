using FluentAssertions;
using FTPClient.Database.Data;
using FTPClient.Database.Repository;
using FTPClient.Models.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FTPClient.Tests.DatabaseTests;

public class DatabaseTests
{
    private readonly AppDbContext _mockAppDbContext;

    private readonly List<Connection> _connections = new List<Connection>()
    {
        new Connection() { Host = "123.123.123.123", Id = 1, Port = 22, Username = "Username1" },
        new Connection() { Host = "321.123.123.321", Id = 2, Port = 22, Username = "Username2" }
    };
    public DatabaseTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(options);
        
        dbContext.Database.EnsureCreated();
        _mockAppDbContext = dbContext;
    }
    [Fact]
    public async Task ConnectionsRepository_GetAllConnections_ReturnsAllConnectionsFromDBIfExistsAndNotEmpty()
    {
        FillDbWithData(_mockAppDbContext, _connections);
        var mockDb = new Mock<IDbContextFactory<AppDbContext>>();
        mockDb.Setup(db => db.CreateDbContextAsync(default))
            .ReturnsAsync(_mockAppDbContext);
        var repo = new ConnectionsRepository(mockDb.Object);
        var result = await repo.GetAllConnections();
        
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(_connections[0]);
        result.Should().ContainEquivalentOf(_connections[1]);
    }
    [Fact]
    public async Task ConnectionsRepository_GetAllConnections_ReturnsEmptyListIfItIsEmpty()
    {
        FillDbWithData(_mockAppDbContext, new List<Connection>());
        var mockDb = new Mock<IDbContextFactory<AppDbContext>>();
        mockDb.Setup(db => db.CreateDbContextAsync(default))
            .ReturnsAsync(_mockAppDbContext);
        var repo = new ConnectionsRepository(mockDb.Object);
        var result = await repo.GetAllConnections();
        
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        result.Should().HaveCount(0);
    }
    [Theory]
    [InlineData("123.123.123.123", "Username1", 1, 22)]
    public async Task ConnectionsRepository_DeleteConnection_ShouldRemoveItem_WhenItemExists(string Host, string Username, int Id, int Port)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) 
            .Options;
        var dbContext = new AppDbContext(options);
        FillDbWithData(dbContext, _connections);
        
        var mockFactory = new Mock<IDbContextFactory<AppDbContext>>();
        mockFactory.Setup(f => f.CreateDbContextAsync(default))
                   .ReturnsAsync(dbContext);
        var repository = new ConnectionsRepository(mockFactory.Object);

        var itemToDelete = new Connection() { Host = Host, Username = Username, Port = Port, Id = Id };
        await repository.DeleteConnection(itemToDelete);
    
        await using (var assertContext = new AppDbContext(options))
        {
            var remainingConnections = await assertContext.Connections.ToListAsync();
            
            remainingConnections.Should().NotBeNull();
            remainingConnections.Should().HaveCount(1);
            remainingConnections.Should().ContainEquivalentOf(_connections[1]);
            remainingConnections.Should().NotContain(c => c.Id == itemToDelete.Id);
        }
    }

    private void FillDbWithData(AppDbContext dbContext, List<Connection> connections)
    {
        dbContext.Connections.AddRange(connections);
        dbContext.SaveChanges();
    }
}