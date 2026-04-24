namespace MovieLibrary.Api.Services;

public class MovieValidator
{
    public string? ValidateReleaseYear(int releaseYear)
    {
        return releaseYear > DateTime.UtcNow.Year
            ? "Release year cannot be in the future."
            : null;
    }
}
