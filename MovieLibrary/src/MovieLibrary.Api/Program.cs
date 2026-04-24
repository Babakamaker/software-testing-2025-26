using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Data;
using MovieLibrary.Api.Repositories;
using MovieLibrary.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<MovieLibraryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MovieLibrary")));

builder.Services.AddScoped<RatingCalculator>();
builder.Services.AddScoped<MovieValidator>();
builder.Services.AddScoped<ReviewRulesValidator>();
builder.Services.AddScoped<ReviewNotificationPolicy>();
builder.Services.AddScoped<ReviewWorkflowService>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

await EnsureDatabaseReadyAsync(app);

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureDatabaseReadyAsync(WebApplication app)
{
    var options = app.Configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();

    if (!options.InitializeSchemaOnStartup && !options.SeedOnStartup)
    {
        return;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MovieLibraryDbContext>();

    if (options.InitializeSchemaOnStartup)
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    if (options.SeedOnStartup)
    {
        await DatabaseSeeder.SeedAsync(dbContext, options.MinimumSeedRecordCount);
    }
}

public partial class Program;
