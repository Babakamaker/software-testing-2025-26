namespace MovieLibrary.Api.Domain;

public class Review
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public int UserId { get; set; }

    public int Score { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public Movie Movie { get; set; } = null!;

    public User User { get; set; } = null!;
}
