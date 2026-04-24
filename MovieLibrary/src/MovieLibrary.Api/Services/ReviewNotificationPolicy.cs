using MovieLibrary.Api.Contracts;
using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Services;

public class ReviewNotificationPolicy
{
    public ReviewNotificationRequest? BuildNotification(Movie movie, User user, Review review)
    {
        var averageRating = movie.Reviews.Count == 0
            ? 0m
            : Math.Round(movie.Reviews.Average(item => (decimal)item.Score), 2, MidpointRounding.AwayFromZero);

        if (movie.Reviews.Count >= 5 && averageRating >= 8.5m && review.Score >= 8)
        {
            return new ReviewNotificationRequest(
                movie.Id,
                movie.Title,
                "featured-movie",
                averageRating,
                movie.Reviews.Count,
                review.Score,
                user.Email);
        }

        if (movie.Reviews.Count >= 3 && averageRating <= 4m && review.Score <= 4)
        {
            return new ReviewNotificationRequest(
                movie.Id,
                movie.Title,
                "quality-alert",
                averageRating,
                movie.Reviews.Count,
                review.Score,
                user.Email);
        }

        return null;
    }
}
