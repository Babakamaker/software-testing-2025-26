using MovieLibrary.Api.Domain;
using MovieLibrary.Api.Services;
using Shouldly;

namespace MovieLibrary.UnitTests;

public class ReviewNotificationPolicyTests
{
    [Fact]
    public void BuildNotification_FeaturedMovieThresholdReached_ReturnsFeaturedNotification()
    {
        var sut = new ReviewNotificationPolicy();
        var movie = CreateMovieWithScores(9, 9, 8, 10, 9);
        var user = new User { Email = "critic@example.com" };
        var review = movie.Reviews.Last();

        var result = sut.BuildNotification(movie, user, review);

        result.ShouldNotBeNull();
        result!.Trigger.ShouldBe("featured-movie");
        result.ReviewCount.ShouldBe(5);
    }

    [Fact]
    public void BuildNotification_LowAverageAfterMultipleReviews_ReturnsQualityAlert()
    {
        var sut = new ReviewNotificationPolicy();
        var movie = CreateMovieWithScores(4, 3, 4);
        var user = new User { Email = "critic@example.com" };
        var review = movie.Reviews.Last();

        var result = sut.BuildNotification(movie, user, review);

        result.ShouldNotBeNull();
        result!.Trigger.ShouldBe("quality-alert");
        result.AverageRating.ShouldBeLessThanOrEqualTo(4m);
    }

    [Fact]
    public void BuildNotification_RulesNotMet_ReturnsNull()
    {
        var sut = new ReviewNotificationPolicy();
        var movie = CreateMovieWithScores(6, 7, 7, 8);
        var user = new User { Email = "critic@example.com" };
        var review = movie.Reviews.Last();

        var result = sut.BuildNotification(movie, user, review);

        result.ShouldBeNull();
    }

    private static Movie CreateMovieWithScores(params int[] scores)
    {
        var movie = new Movie
        {
            Id = 1,
            Title = "Test Movie",
            Director = "Test Director",
            Genre = Genre.Drama,
            ReleaseYear = 2020,
            DurationMinutes = 120,
        };

        foreach (var score in scores)
        {
            movie.Reviews.Add(new Review
            {
                Id = movie.Reviews.Count + 1,
                MovieId = movie.Id,
                UserId = movie.Reviews.Count + 1,
                Score = score,
                Comment = "This is a sufficiently detailed review comment.",
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        return movie;
    }
}
