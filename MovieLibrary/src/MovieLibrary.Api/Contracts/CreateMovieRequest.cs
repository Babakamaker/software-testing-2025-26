using System.ComponentModel.DataAnnotations;
using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Contracts;

public class CreateMovieRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Director { get; init; } = string.Empty;

    [Required]
    public Genre Genre { get; init; }

    [Range(1888, 3000)]
    public int ReleaseYear { get; init; }

    [Range(1, 600)]
    public int DurationMinutes { get; init; }
}
