namespace MovieLibrary.Api.Domain;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
