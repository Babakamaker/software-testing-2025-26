using System.ComponentModel.DataAnnotations;

namespace MovieLibrary.Api.Contracts;

public class CreateReviewRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; init; }

    [Range(1, 10)]
    public int Score { get; init; }

    [Required]
    [MaxLength(2_000)]
    public string Comment { get; init; } = string.Empty;
}
