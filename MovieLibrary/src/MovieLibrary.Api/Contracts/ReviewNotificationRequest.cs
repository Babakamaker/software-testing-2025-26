namespace MovieLibrary.Api.Contracts;

public record ReviewNotificationRequest(
    int MovieId,
    string MovieTitle,
    string Trigger,
    decimal AverageRating,
    int ReviewCount,
    int LatestScore,
    string RecipientEmail);
