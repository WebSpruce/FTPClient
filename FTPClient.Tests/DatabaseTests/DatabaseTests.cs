using FluentAssertions;
using FTPClient.Database.Data;
using FTPClient.Database.Repository;
using FTPClient.Models.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FTPClient.Tests.DatabaseTests;

public class DatabaseTests
{
    private static readonly List<Connection> SeedConnections =
    [
        new Connection { Host = "123.123.123.123", Id = 1, Port = 22, Username = "Username1" },
        new Connection { Host = "321.123.123.321", Id = 2, Port = 22, Username = "Username2" }
    ];

    [Fact]
    public async Task GetAllConnections_ReturnsAllConnections_WhenDatabaseHasData()
    {
        // Arrange
        var factory = CreateFactory();
        await SeedAsync(factory, SeedConnections);
        var cts = GetCancellationToken();

        var repository = new ConnectionsRepository(factory);

        // Act
        var result = await repository.GetAllConnections(cts);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(SeedConnections[0]);
        result.Should().ContainEquivalentOf(SeedConnections[1]);
    }

    [Fact]
    public async Task GetAllConnections_ReturnsEmptyList_WhenDatabaseIsEmpty()
    {
        var factory = CreateFactory();
        var repository = new ConnectionsRepository(factory);
        var cts = GetCancellationToken();

        var result = await repository.GetAllConnections(cts);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteConnection_RemovesItem_WhenItemExists()
    {
        var factory = CreateFactory();
        await SeedAsync(factory, SeedConnections);
        var cts = GetCancellationToken();

        var repository = new ConnectionsRepository(factory);
        var itemToDelete = new Connection
        {
            Host = "123.123.123.123",
            Username = "Username1",
            Port = 22,
            Id = 1
        };
        
        await repository.DeleteConnection(itemToDelete, cts);
        
        await using var assertContext = await factory.CreateDbContextAsync();
        var remainingConnections = await assertContext.Connections.ToListAsync();

        remainingConnections.Should().HaveCount(1);
        remainingConnections.Should().ContainSingle(c => c.Id == 2);
        remainingConnections.Should().NotContain(c => c.Id == itemToDelete.Id);
    }

    [Fact]
    public async Task SaveConnection_AddsItem_WhenConnectionIsValid()
    {
        var factory = CreateFactory();
        var repository = new ConnectionsRepository(factory);
        var cts = GetCancellationToken();

        var connection = new Connection
        {
            Host = "111.222.333.444",
            Id = 3,
            Port = 22,
            Username = "Username3"
        };

        await repository.SaveConnection(connection, cts);

        await using var assertContext = await factory.CreateDbContextAsync();
        var connections = await assertContext.Connections.ToListAsync();

        connections.Should().HaveCount(1);
        connections.Should().ContainEquivalentOf(connection);
    }

    private static TestDbContextFactory CreateFactory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContextFactory(options);
    }

    private static async Task SeedAsync(
        IDbContextFactory<AppDbContext> factory,
        IEnumerable<Connection> connections)
    {
        await using var context = await factory.CreateDbContextAsync();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await context.Connections.AddRangeAsync(connections);
        await context.SaveChangesAsync();
    }

    private static CancellationToken GetCancellationToken()
    {
        var ctsSource = new CancellationTokenSource();
        return ctsSource.Token;
    }

    private sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext()
        {
            return new AppDbContext(_options);
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AppDbContext(_options));
        }
    }
}