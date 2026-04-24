using MovieLibrary.Api.Contracts;
using MovieLibrary.Api.Domain;
using MovieLibrary.Api.Repositories;

namespace MovieLibrary.Api.Services;

public class ReviewWorkflowService(
    IMovieRepository movieRepository,
    IUserRepository userRepository,
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork,
    ReviewRulesValidator reviewRulesValidator,
    ReviewNotificationPolicy reviewNotificationPolicy)
{
    public async Task<ReviewSubmissionResult> SubmitAsync(
        int movieId,
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await movieRepository.GetByIdWithReviewsAsync(movieId, cancellationToken);
        if (movie is null)
        {
            return new ReviewSubmissionResult
            {
                MovieNotFound = true,
            };
        }

        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Validation(nameof(request.UserId), "User does not exist.");
        }

        var scoreError = reviewRulesValidator.ValidateScore(request.Score);
        if (scoreError is not null)
        {
            return Validation(nameof(request.Score), scoreError);
        }

        var commentError = reviewRulesValidator.ValidateComment(request.Score, request.Comment);
        if (commentError is not null)
        {
            return Validation(nameof(request.Comment), commentError);
        }

        var duplicateError = await reviewRulesValidator.ValidateDuplicateReviewAsync(
            reviewRepository,
            movieId,
            request.UserId,
            cancellationToken);

        if (duplicateError is not null)
        {
            return new ReviewSubmissionResult
            {
                IsDuplicate = true,
                ErrorMessage = duplicateError,
            };
        }

        var review = new Review
        {
            MovieId = movieId,
            UserId = request.UserId,
            Score = request.Score,
            Comment = request.Comment.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            Movie = movie,
            User = user,
        };

        await reviewRepository.AddAsync(review, cancellationToken);
        movie.Reviews.Add(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notification delivery removed. The policy still returns a notification shape
        // for inspection, but this service no longer sends it to any external system.
        _ = reviewNotificationPolicy.BuildNotification(movie, user, review);

        return new ReviewSubmissionResult
        {
            IsSuccess = true,
            Review = new ReviewResponse(
                review.Id,
                review.MovieId,
                review.UserId,
                user.Username,
                review.Score,
                review.Comment,
                review.CreatedAt),
        };
    }

    private static ReviewSubmissionResult Validation(string field, string message) =>
        new()
        {
            ErrorField = field,
            ErrorMessage = message,
        };
}
