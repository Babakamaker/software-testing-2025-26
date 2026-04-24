using MovieLibrary.Api.Contracts;

namespace MovieLibrary.Api.Services;

public class ReviewSubmissionResult
{
    public bool IsSuccess { get; init; }

    public bool MovieNotFound { get; init; }

    public bool IsDuplicate { get; init; }

    public string? ErrorField { get; init; }

    public string? ErrorMessage { get; init; }

    public ReviewResponse? Review { get; init; }
}
