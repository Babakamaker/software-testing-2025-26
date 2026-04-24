using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieLibrary.Api.Data;
using Testcontainers.PostgreSql;

namespace MovieLibrary.IntegrationTests;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("movielibrary_integration")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public MovieLibraryApiFactory Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        Factory = new MovieLibraryApiFactory(ConnectionString);
        Client = Factory.CreateClient();
        await ResetDatabaseAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(dbContext, 10_000);

        using var scope = Factory.Services.CreateScope();
    }

    public MovieLibraryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MovieLibraryDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new MovieLibraryDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        await _container.DisposeAsync();
    }
}
