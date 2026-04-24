using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Data;

public class MovieLibraryDbContext(DbContextOptions<MovieLibraryDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var movie = modelBuilder.Entity<Movie>();
        movie.ToTable("movies");
        movie.HasKey(entity => entity.Id);
        movie.Property(entity => entity.Title).HasMaxLength(200).IsRequired();
        movie.Property(entity => entity.Director).HasMaxLength(120).IsRequired();
        movie.Property(entity => entity.Genre).HasConversion<string>().HasMaxLength(32).IsRequired();
        movie.Property(entity => entity.ReleaseYear).IsRequired();
        movie.Property(entity => entity.DurationMinutes).IsRequired();
        movie.HasIndex(entity => entity.Genre);
        movie.HasIndex(entity => entity.ReleaseYear);
        movie.HasMany(entity => entity.Reviews)
            .WithOne(entity => entity.Movie)
            .HasForeignKey(entity => entity.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        var review = modelBuilder.Entity<Review>();
        review.ToTable("reviews", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("ck_reviews_score_range", "\"Score\" >= 1 AND \"Score\" <= 10");
        });
        review.HasKey(entity => entity.Id);
        review.Property(entity => entity.Score).IsRequired();
        review.Property(entity => entity.Comment).HasMaxLength(2_000).IsRequired();
        review.Property(entity => entity.CreatedAt).IsRequired();
        review.HasIndex(entity => new { entity.MovieId, entity.UserId }).IsUnique();
        review.HasOne(entity => entity.User)
            .WithMany(entity => entity.Reviews)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        var user = modelBuilder.Entity<User>();
        user.ToTable("users");
        user.HasKey(entity => entity.Id);
        user.Property(entity => entity.Username).HasMaxLength(80).IsRequired();
        user.Property(entity => entity.Email).HasMaxLength(200).IsRequired();
        user.HasIndex(entity => entity.Username).IsUnique();
        user.HasIndex(entity => entity.Email).IsUnique();
    }
}
