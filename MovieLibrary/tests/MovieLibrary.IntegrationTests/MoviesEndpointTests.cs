using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieLibrary.Api.Contracts;
using MovieLibrary.Api.Domain;
using Shouldly;

namespace MovieLibrary.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class MoviesEndpointTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly Fixture _fixture = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public ValueTask InitializeAsync() => new(fixture.ResetDatabaseAsync());

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetMovies_FilteredByGenreAndYear_ReturnsMatchingMoviesOnly()
    {
        await using var dbContext = fixture.CreateDbContext();
        var expectedMovie = await dbContext.Movies.AsNoTracking().FirstAsync();

        var response = await fixture.Client.GetAsync($"/api/movies?genre={expectedMovie.Genre}&year={expectedMovie.ReleaseYear}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var movies = await response.Content.ReadFromJsonAsync<List<MovieDetailsResponse>>(JsonOptions);
        movies.ShouldNotBeNull();
        movies.ShouldNotBeEmpty();
        movies.All(movie => movie.Genre == expectedMovie.Genre && movie.ReleaseYear == expectedMovie.ReleaseYear).ShouldBeTrue();
    }

    [Fact]
    public async Task GetMovie_ExistingMovie_ReturnsAverageRatingPayload()
    {
        await using var dbContext = fixture.CreateDbContext();
        var movie = await dbContext.Movies
            .AsNoTracking()
            .Where(item => item.Reviews.Any())
            .Select(item => new
            {
                item.Id,
                ExpectedAverage = decimal.Round(item.Reviews.Average(review => (decimal)review.Score), 2, MidpointRounding.AwayFromZero),
            })
            .FirstAsync();

        var response = await fixture.Client.GetAsync($"/api/movies/{movie.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<MovieDetailsResponse>(JsonOptions);
        payload.ShouldNotBeNull();
        payload!.Rating.ShouldBe(movie.ExpectedAverage);
    }

    [Fact]
    public async Task CreateMovie_ValidRequest_ReturnsCreatedMovie()
    {
        var request = new CreateMovieRequest
        {
            Title = _fixture.Create<string>(),
            Director = _fixture.Create<string>(),
            Genre = Genre.Thriller,
            ReleaseYear = DateTime.UtcNow.Year,
            DurationMinutes = 123,
        };

        var response = await fixture.Client.PostAsJsonAsync("/api/movies", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<MovieDetailsResponse>(JsonOptions);
        payload.ShouldNotBeNull();
        payload!.Title.ShouldBe(request.Title);
    }

    [Fact]
    public async Task CreateMovie_FutureReleaseYear_ReturnsBadRequest()
    {
        var request = new CreateMovieRequest
        {
            Title = _fixture.Create<string>(),
            Director = _fixture.Create<string>(),
            Genre = Genre.Thriller,
            ReleaseYear = DateTime.UtcNow.Year + 1,
            DurationMinutes = 123,
        };

        var response = await fixture.Client.PostAsJsonAsync("/api/movies", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateMovie_ExistingMovie_UpdatesState()
    {
        var createResponse = await fixture.Client.PostAsJsonAsync("/api/movies", new CreateMovieRequest
        {
            Title = _fixture.Create<string>(),
            Director = _fixture.Create<string>(),
            Genre = Genre.Comedy,
            ReleaseYear = DateTime.UtcNow.Year - 1,
            DurationMinutes = 90,
        });
        var created = await createResponse.Content.ReadFromJsonAsync<MovieDetailsResponse>(JsonOptions);

        var updateResponse = await fixture.Client.PutAsJsonAsync($"/api/movies/{created!.Id}", new UpdateMovieRequest
        {
            Title = "Updated title",
            Director = "Updated director",
            Genre = Genre.Drama,
            ReleaseYear = DateTime.UtcNow.Year - 1,
            DurationMinutes = 150,
        });

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updated = await fixture.Client.GetFromJsonAsync<MovieDetailsResponse>($"/api/movies/{created.Id}", JsonOptions);
        updated.ShouldNotBeNull();
        updated!.Title.ShouldBe("Updated title");
        updated.Director.ShouldBe("Updated director");
        updated.DurationMinutes.ShouldBe(150);
    }

    [Fact]
    public async Task DeleteMovie_ExistingMovie_ReturnsNotFoundOnSubsequentFetch()
    {
        var createResponse = await fixture.Client.PostAsJsonAsync("/api/movies", new CreateMovieRequest
        {
            Title = _fixture.Create<string>(),
            Director = _fixture.Create<string>(),
            Genre = Genre.Comedy,
            ReleaseYear = DateTime.UtcNow.Year - 1,
            DurationMinutes = 90,
        });
        var created = await createResponse.Content.ReadFromJsonAsync<MovieDetailsResponse>(JsonOptions);

        var deleteResponse = await fixture.Client.DeleteAsync($"/api/movies/{created!.Id}");
        var getResponse = await fixture.Client.GetAsync($"/api/movies/{created.Id}");

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReview_ValidRequest_UpdatesAverageRating()
    {
        await using var dbContext = fixture.CreateDbContext();
        var users = await dbContext.Users.AsNoTracking().OrderBy(user => user.Id).Take(2).ToListAsync();
        var createMovieResponse = await fixture.Client.PostAsJsonAsync("/api/movies", new CreateMovieRequest
        {
            Title = _fixture.Create<string>(),
            Director = _fixture.Create<string>(),
            Genre = Genre.Action,
            ReleaseYear = DateTime.UtcNow.Year - 1,
            DurationMinutes = 110,
        });
        var movie = await createMovieResponse.Content.ReadFromJsonAsync<MovieDetailsResponse>(JsonOptions);

        var firstReviewResponse = await fixture.Client.PostAsJsonAsync($"/api/movies/{movie!.Id}/reviews", new CreateReviewRequest
        {
            UserId = users[0].Id,
            Score = 6,
            Comment = "This is a sufficiently detailed first review comment."
        });
        var secondReviewResponse = await fixture.Client.PostAsJsonAsync($"/api/movies/{movie.Id}/reviews", new CreateReviewRequest
        {
            UserId = users[1].Id,
            Score = 9,
            Comment = "This is a sufficiently detailed second review comment."
        });

        firstReviewResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        secondReviewResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var updatedMovie = await fixture.Client.GetFromJsonAsync<MovieDetailsResponse>($"/api/movies/{movie.Id}", JsonOptions);
        updatedMovie.ShouldNotBeNull();
        updatedMovie!.Rating.ShouldBe(7.5m);
        updatedMovie.ReviewCount.ShouldBe(2);
    }

    [Fact]
    public async Task CreateReview_DuplicateUserReview_ReturnsConflict()
    {
        await using var dbContext = fixture.CreateDbContext();
        var user = await dbContext.Users.AsNoTracking().OrderBy(item => item.Id).FirstAsync();
        var createMovieResponse = await fixture.Client.PostAsJsonAsync("/api/movies", new CreateMovieRequest
        {
            Title = _fixture.Create<string>(),
            Director = _fixture.Create<string>(),
            Genre = Genre.Action,
            ReleaseYear = DateTime.UtcNow.Year - 1,
            DurationMinutes = 110,
        });
        var movie = await createMovieResponse.Content.ReadFromJsonAsync<MovieDetailsResponse>(JsonOptions);

        await fixture.Client.PostAsJsonAsync($"/api/movies/{movie!.Id}/reviews", new CreateReviewRequest
        {
            UserId = user.Id,
            Score = 8,
            Comment = "This is a sufficiently detailed first review comment."
        });

        var duplicateResponse = await fixture.Client.PostAsJsonAsync($"/api/movies/{movie.Id}/reviews", new CreateReviewRequest
        {
            UserId = user.Id,
            Score = 9,
            Comment = "This is a sufficiently detailed duplicate review comment."
        });

        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateReview_LowScoreShortComment_ReturnsBadRequest()
    {
        await using var dbContext = fixture.CreateDbContext();
        var user = await dbContext.Users.AsNoTracking().OrderBy(item => item.Id).FirstAsync();
        var createMovieResponse = await fixture.Client.PostAsJsonAsync("/api/movies", new CreateMovieRequest
        {
            Title = _fixture.Create<string>(),
            Director = _fixture.Create<string>(),
            Genre = Genre.Action,
            ReleaseYear = DateTime.UtcNow.Year - 1,
            DurationMinutes = 110,
        });
        var movie = await createMovieResponse.Content.ReadFromJsonAsync<MovieDetailsResponse>(JsonOptions);

        var response = await fixture.Client.PostAsJsonAsync($"/api/movies/{movie!.Id}/reviews", new CreateReviewRequest
        {
            UserId = user.Id,
            Score = 2,
            Comment = "too short"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

}
