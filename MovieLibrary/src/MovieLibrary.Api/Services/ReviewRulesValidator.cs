using MovieLibrary.Api.Repositories;

namespace MovieLibrary.Api.Services;

public class ReviewRulesValidator
{
    public string? ValidateScore(int score)
    {
        return score is < 1 or > 10
            ? "Score must be between 1 and 10."
            : null;
    }

    public string? ValidateComment(int score, string comment)
    {
        return comment.Trim().Length < 20
            ? "Review must include at least 20 characters in the comment."
            : null;
    }

    public async Task<string?> ValidateDuplicateReviewAsync(
        IReviewRepository reviewRepository,
        int movieId,
        int userId,
        CancellationToken cancellationToken)
    {
        var exists = await reviewRepository.ExistsForMovieAndUserAsync(movieId, userId, cancellationToken);

        return exists
            ? "A user can leave only one review per movie."
            : null;
    }
}
