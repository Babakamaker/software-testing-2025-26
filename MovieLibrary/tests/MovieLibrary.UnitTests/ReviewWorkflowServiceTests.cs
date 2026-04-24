using AutoFixture;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Contracts;
using MovieLibrary.Api.Data;
using MovieLibrary.Api.Domain;
using MovieLibrary.Api.Repositories;
using MovieLibrary.Api.Services;
using NSubstitute;
using Shouldly;

namespace MovieLibrary.UnitTests;

public class ReviewWorkflowServiceTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task SubmitAsync_MovieDoesNotExist_ReturnsMovieNotFound()
    {
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(999, new CreateReviewRequest
        {
            UserId = 1,
            Score = 8,
            Comment = "This is a sufficiently detailed review comment."
        }, TestContext.Current.CancellationToken);

        result.MovieNotFound.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task SubmitAsync_UserDoesNotExist_ReturnsValidationFailure()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Movies.Add(CreateMovie());
        await dbContext.SaveChangesAsync();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(1, new CreateReviewRequest
        {
            UserId = 404,
            Score = 8,
            Comment = "This is a sufficiently detailed review comment."
        }, TestContext.Current.CancellationToken);

        result.ErrorField.ShouldBe(nameof(CreateReviewRequest.UserId));
        result.ErrorMessage.ShouldBe("User does not exist.");
    }

    [Fact]
    public async Task SubmitAsync_InvalidScore_ReturnsValidationFailure()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Movies.Add(CreateMovie());
        dbContext.Users.Add(CreateUser());
        await dbContext.SaveChangesAsync();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(1, new CreateReviewRequest
        {
            UserId = 1,
            Score = 11,
            Comment = "This is a sufficiently detailed review comment."
        }, TestContext.Current.CancellationToken);

        result.ErrorField.ShouldBe(nameof(CreateReviewRequest.Score));
        result.ErrorMessage.ShouldBe("Score must be between 1 and 10.");
    }

    [Fact]
    public async Task SubmitAsync_ShortComment_ReturnsValidationFailure()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Movies.Add(CreateMovie());
        dbContext.Users.Add(CreateUser());
        await dbContext.SaveChangesAsync();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(1, new CreateReviewRequest
        {
            UserId = 1,
            Score = 2,
            Comment = "too short"
        }, TestContext.Current.CancellationToken);

        result.ErrorField.ShouldBe(nameof(CreateReviewRequest.Comment));
        result.ErrorMessage.ShouldBe("Review must include at least 20 characters in the comment.");
    }

    [Fact]
    public async Task SubmitAsync_DuplicateReview_ReturnsDuplicateFailure()
    {
        await using var dbContext = CreateDbContext();
        var movie = CreateMovie();
        var user = CreateUser();
        dbContext.Movies.Add(movie);
        dbContext.Users.Add(user);
        dbContext.Reviews.Add(new Review
        {
            MovieId = 1,
            UserId = 1,
            Score = 8,
            Comment = "This is a sufficiently detailed review comment.",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(1, new CreateReviewRequest
        {
            UserId = 1,
            Score = 9,
            Comment = "This is another sufficiently detailed review comment."
        }, TestContext.Current.CancellationToken);

        result.IsDuplicate.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("A user can leave only one review per movie.");
    }

    [Fact]
    public async Task SubmitAsync_ValidReviewWithoutNotification_PersistsReview()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Movies.Add(CreateMovie());
        dbContext.Users.Add(CreateUser());
        await dbContext.SaveChangesAsync();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(1, new CreateReviewRequest
        {
            UserId = 1,
            Score = 7,
            Comment = "This is a sufficiently detailed review comment."
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Review.ShouldNotBeNull();
        dbContext.Reviews.Count().ShouldBe(1);
    }

    [Fact]
    public async Task SubmitAsync_FeaturedMovieThresholdReached_ShouldReturnSuccess()
    {
        await using var dbContext = CreateDbContext();
        var movie = CreateMovie();
        var user = CreateUser();
        dbContext.Movies.Add(movie);
        dbContext.Users.Add(user);
        SeedReviews(dbContext, movie.Id, [9, 9, 8, 10], 100);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(1, new CreateReviewRequest
        {
            UserId = 1,
            Score = 9,
            Comment = "This is a sufficiently detailed review comment."
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task SubmitAsync_QualityAlertThresholdReached_ShouldReturnSuccess()
    {
        await using var dbContext = CreateDbContext();
        var movie = CreateMovie();
        var user = CreateUser();
        dbContext.Movies.Add(movie);
        dbContext.Users.Add(user);
        SeedReviews(dbContext, movie.Id, [4, 3], 100);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut(dbContext);

        var result = await sut.SubmitAsync(1, new CreateReviewRequest
        {
            UserId = 1,
            Score = 4,
            Comment = "This is a sufficiently detailed review comment."
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
    }

    private ReviewWorkflowService CreateSut(MovieLibraryDbContext dbContext)
    {
        return new ReviewWorkflowService(
            new MovieRepository(dbContext),
            new UserRepository(dbContext),
            new ReviewRepository(dbContext),
            new UnitOfWork(dbContext),
            new ReviewRulesValidator(),
            new ReviewNotificationPolicy());
    }

    private MovieLibraryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MovieLibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MovieLibraryDbContext(options);
    }

    private Movie CreateMovie() => new()
    {
        Id = 1,
        Title = _fixture.Create<string>(),
        Director = _fixture.Create<string>(),
        Genre = Genre.Action,
        ReleaseYear = 2024,
        DurationMinutes = 120
    };

    private User CreateUser() => new()
    {
        Id = 1,
        Username = $"user-{Guid.NewGuid():N}",
        Email = $"user-{Guid.NewGuid():N}@example.com"
    };

    private static void SeedReviews(MovieLibraryDbContext dbContext, int movieId, int[] scores, int userSeed)
    {
        for (var index = 0; index < scores.Length; index++)
        {
            var userId = userSeed + index;
            dbContext.Users.Add(new User
            {
                Id = userId,
                Username = $"seed-user-{userId}",
                Email = $"seed-user-{userId}@example.com"
            });
            dbContext.Reviews.Add(new Review
            {
                MovieId = movieId,
                UserId = userId,
                Score = scores[index],
                Comment = "This is a sufficiently detailed seeded review comment.",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }
}
