namespace MovieLibrary.Api.Domain;

public class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Director { get; set; } = string.Empty;

    public Genre Genre { get; set; }

    public int ReleaseYear { get; set; }

    public int DurationMinutes { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
