using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovieLibrary.Api.Data;

namespace MovieLibrary.IntegrationTests;

public class MovieLibraryApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MovieLibrary"] = connectionString,
                ["Database:InitializeSchemaOnStartup"] = "false",
                ["Database:SeedOnStartup"] = "false",
                ["Database:MinimumSeedRecordCount"] = "10000",
                ["NotificationService:BaseUrl"] = "https://notifications.example.com",
                ["NotificationService:ApiKey"] = "testing-key",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<MovieLibraryDbContext>));
            services.RemoveAll(typeof(MovieLibraryDbContext));

            services.AddDbContext<MovieLibraryDbContext>(options =>
                options.UseNpgsql(connectionString));
        });
    }
}
