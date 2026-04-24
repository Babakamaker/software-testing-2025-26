namespace MovieLibrary.Api.Contracts;

public record ReviewResponse(
    int Id,
    int MovieId,
    int UserId,
    string Username,
    int Score,
    string Comment,
    DateTimeOffset CreatedAt);
