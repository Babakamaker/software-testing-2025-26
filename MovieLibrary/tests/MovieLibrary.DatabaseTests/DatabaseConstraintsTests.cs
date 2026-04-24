using AutoFixture;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Domain;
using Npgsql;
using Shouldly;

namespace MovieLibrary.DatabaseTests;

[Collection(nameof(DatabaseCollection))]
public class DatabaseConstraintsTests(PostgresDatabaseFixture fixture) : IAsyncLifetime
{
    private readonly Fixture _fixture = new();

    public ValueTask InitializeAsync() => new(fixture.ResetDatabaseAsync());

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SaveChangesAsync_DuplicateReviewForSameUserAndMovie_ThrowsDbUpdateException()
    {
        await using var dbContext = fixture.CreateDbContext();
        var movie = CreateMovie(Genre.Comedy, DateTime.UtcNow.Year - 1, 97);
        var user = CreateUser("user");
        dbContext.Movies.Add(movie);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        dbContext.Reviews.Add(new Review
        {
            MovieId = movie.Id,
            UserId = user.Id,
            Score = 8,
            Comment = _fixture.Create<string>(),
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        dbContext.Reviews.Add(new Review
        {
            MovieId = movie.Id,
            UserId = user.Id,
            Score = 9,
            Comment = _fixture.Create<string>(),
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await Should.ThrowAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_DeletingMovie_CascadesToReviews()
    {
        await using var dbContext = fixture.CreateDbContext();
        var movie = CreateMovie(Genre.Documentary, DateTime.UtcNow.Year - 2, 101);
        var user = CreateUser("critic");
        dbContext.Movies.Add(movie);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var review = new Review
        {
            MovieId = movie.Id,
            UserId = user.Id,
            Score = 7,
            Comment = _fixture.Create<string>(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync();

        var reviewStillExists = await dbContext.Reviews.AnyAsync(entity => entity.Id == review.Id);
        reviewStillExists.ShouldBeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync_ScoreOutsideAllowedRange_ThrowsDbUpdateException()
    {
        await using var dbContext = fixture.CreateDbContext();
        var movie = CreateMovie(Genre.Action, DateTime.UtcNow.Year - 1, 100);
        var user = CreateUser("range");
        dbContext.Movies.Add(movie);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        dbContext.Reviews.Add(new Review
        {
            MovieId = movie.Id,
            UserId = user.Id,
            Score = 11,
            Comment = "This is a sufficiently detailed review comment.",
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await Should.ThrowAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateUsername_ThrowsDbUpdateException()
    {
        await using var dbContext = fixture.CreateDbContext();

        dbContext.Users.Add(CreateUser("same-name", "same-name@example.com"));
        dbContext.Users.Add(CreateUser("same-name", "different@example.com"));

        await Should.ThrowAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateEmail_ThrowsDbUpdateException()
    {
        await using var dbContext = fixture.CreateDbContext();

        dbContext.Users.Add(CreateUser("email-a", "shared@example.com"));
        dbContext.Users.Add(CreateUser("email-b", "shared@example.com"));

        await Should.ThrowAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task ExecuteSqlRawAsync_ReviewWithMissingMovie_ThrowsPostgresException()
    {
        await using var dbContext = fixture.CreateDbContext();
        var user = CreateUser("foreign");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var act = async () => await dbContext.Database.ExecuteSqlRawAsync(
            @"INSERT INTO reviews (""MovieId"", ""UserId"", ""Score"", ""Comment"", ""CreatedAt"") VALUES ({0}, {1}, {2}, {3}, {4})",
            999999,
            user.Id,
            5,
            "This is a sufficiently detailed review comment.",
            DateTimeOffset.UtcNow);

        await Should.ThrowAsync<PostgresException>(act);
    }

    private Movie CreateMovie(Genre genre, int releaseYear, int durationMinutes) => new()
    {
        Title = _fixture.Create<string>(),
        Director = _fixture.Create<string>(),
        Genre = genre,
        ReleaseYear = releaseYear,
        DurationMinutes = durationMinutes,
    };

    private User CreateUser(string usernameSuffix, string? email = null) => new()
    {
        Username = usernameSuffix,
        Email = email ?? $"{Guid.NewGuid():N}@example.com",
    };
}
