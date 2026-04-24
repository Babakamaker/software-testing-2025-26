using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Contracts;

public record MovieDetailsResponse(
    int Id,
    string Title,
    string Director,
    Genre Genre,
    int ReleaseYear,
    int DurationMinutes,
    decimal Rating,
    int ReviewCount);
