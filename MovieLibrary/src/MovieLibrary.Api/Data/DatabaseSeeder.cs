using AutoFixture;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        MovieLibraryDbContext dbContext,
        int minimumSeedRecordCount,
        CancellationToken cancellationToken = default)
    {
        var existingRecordCount =
            await dbContext.Movies.CountAsync(cancellationToken) +
            await dbContext.Users.CountAsync(cancellationToken) +
            await dbContext.Reviews.CountAsync(cancellationToken);

        if (existingRecordCount >= minimumSeedRecordCount)
        {
            return;
        }

        dbContext.Reviews.RemoveRange(dbContext.Reviews);
        dbContext.Movies.RemoveRange(dbContext.Movies);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync(cancellationToken);

        const int movieCount = 2_500;
        const int userCount = 3_000;
        const int reviewCount = 6_500;

        var fixture = new Fixture();
        var random = new Random(20260420);
        var currentYear = DateTime.UtcNow.Year;
        var genres = Enum.GetValues<Genre>();

        var titlePrefixes = new[]
        {
            "Silent", "Broken", "Golden", "Shadow", "Burning", "Hidden", "Last", "Neon", "Midnight", "Crimson",
        };

        var titleSubjects = new[]
        {
            "Empire", "Harbor", "Sky", "Road", "Signal", "Archive", "Season", "Promise", "Frontier", "Memory",
        };

        var directorFirstNames = new[]
        {
            "Alex", "Maya", "Daniel", "Ivy", "Roman", "Olivia", "Victor", "Sofia", "Noah", "Elena",
        };

        var directorLastNames = new[]
        {
            "Stone", "Reeves", "Caldwell", "Marin", "Hart", "Fox", "Santos", "Khan", "Byrne", "Novak",
        };

        var usernamePrefixes = new[]
        {
            "cinema", "frame", "screen", "pilot", "critic", "story", "reel", "scene", "take", "focus",
        };

        var usernameSuffixes = new[]
        {
            "fox", "owl", "wave", "spark", "echo", "glow", "dash", "pixel", "note", "trail",
        };

        var commentFragments = new[]
        {
            "Strong performances and clean pacing.",
            "A thoughtful genre entry with a confident ending.",
            "Visually sharp and easy to recommend.",
            "The script drags in the middle, but the finale lands.",
            "Memorable characters and a satisfying score.",
            "A crowd-pleaser with a few rough edges.",
            "Surprisingly emotional and very well directed.",
            "A polished watch that rewards attention.",
        };

        var users = Enumerable.Range(1, userCount)
            .Select(index => new User
            {
                Username = $"{usernamePrefixes[random.Next(usernamePrefixes.Length)]}{usernameSuffixes[random.Next(usernameSuffixes.Length)]}{index}",
                Email = $"viewer{index}@example.com",
            })
            .ToList();

        var movies = Enumerable.Range(1, movieCount)
            .Select(index => new Movie
            {
                Title = $"{titlePrefixes[random.Next(titlePrefixes.Length)]} {titleSubjects[random.Next(titleSubjects.Length)]} {index}",
                Director = $"{directorFirstNames[random.Next(directorFirstNames.Length)]} {directorLastNames[random.Next(directorLastNames.Length)]}",
                Genre = genres[random.Next(genres.Length)],
                ReleaseYear = random.Next(1980, currentYear + 1),
                DurationMinutes = random.Next(80, 181),
            })
            .ToList();

        dbContext.Users.AddRange(users);
        dbContext.Movies.AddRange(movies);
        await dbContext.SaveChangesAsync(cancellationToken);

        var reviews = new List<Review>(reviewCount);
        var usedPairs = new HashSet<(int MovieId, int UserId)>();

        while (reviews.Count < reviewCount)
        {
            var movie = movies[random.Next(movies.Count)];
            var user = users[random.Next(users.Count)];
            var pair = (movie.Id, user.Id);

            if (!usedPairs.Add(pair))
            {
                continue;
            }

            reviews.Add(new Review
            {
                MovieId = movie.Id,
                UserId = user.Id,
                Score = random.Next(1, 11),
                Comment = $"{fixture.Create<string>()[..8]} {commentFragments[random.Next(commentFragments.Length)]}",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 2_000)),
            });
        }

        dbContext.Reviews.AddRange(reviews);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
