using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Data;
using Testcontainers.PostgreSql;

namespace MovieLibrary.DatabaseTests;

public class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("movielibrary_database")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        await ResetDatabaseAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(dbContext, 10_000);
    }

    public MovieLibraryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MovieLibraryDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new MovieLibraryDbContext(options);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
