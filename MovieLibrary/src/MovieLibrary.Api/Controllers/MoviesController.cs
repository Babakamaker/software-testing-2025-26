using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Contracts;
using MovieLibrary.Api.Domain;
using MovieLibrary.Api.Repositories;
using MovieLibrary.Api.Services;

namespace MovieLibrary.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly MovieValidator _movieValidator;
    private readonly RatingCalculator _ratingCalculator;
    private readonly ReviewWorkflowService _reviewWorkflowService;

    public MoviesController(
        IMovieRepository movieRepository,
        IReviewRepository reviewRepository,
        MovieValidator movieValidator,
        RatingCalculator ratingCalculator,
        ReviewWorkflowService reviewWorkflowService)
    {
        _movieRepository = movieRepository;
        _reviewRepository = reviewRepository;
        _movieValidator = movieValidator;
        _ratingCalculator = ratingCalculator;
        _reviewWorkflowService = reviewWorkflowService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MovieDetailsResponse>>> GetMovies(
        [FromQuery] Genre? genre,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var query = _movieRepository.Query()
            .AsNoTracking()
            .AsQueryable();

        if (genre.HasValue)
        {
            query = query.Where(movie => movie.Genre == genre.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(movie => movie.ReleaseYear == year.Value);
        }

        var movies = await query
            .OrderBy(movie => movie.Title)
            .Select(movie => new MovieAggregate(
                movie.Id,
                movie.Title,
                movie.Director,
                movie.Genre,
                movie.ReleaseYear,
                movie.DurationMinutes,
                movie.Reviews.Select(review => (decimal?)review.Score).Average(),
                movie.Reviews.Count))
            .ToListAsync(cancellationToken);

        return Ok(movies.Select(MapMovieDetails).ToArray());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDetailsResponse>> GetMovie(int id, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.Query()
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new MovieAggregate(
                entity.Id,
                entity.Title,
                entity.Director,
                entity.Genre,
                entity.ReleaseYear,
                entity.DurationMinutes,
                entity.Reviews.Select(review => (decimal?)review.Score).Average(),
                entity.Reviews.Count))
            .SingleOrDefaultAsync(cancellationToken);

        if (movie is null)
        {
            return NotFound();
        }

        return Ok(MapMovieDetails(movie));
    }

    [HttpPost]
    public async Task<ActionResult<MovieDetailsResponse>> CreateMovie(
        [FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var releaseYearError = _movieValidator.ValidateReleaseYear(request.ReleaseYear);
        if (releaseYearError is not null)
        {
            return ValidationFailure(nameof(request.ReleaseYear), releaseYearError);
        }

        var movie = new Movie
        {
            Title = request.Title.Trim(),
            Director = request.Director.Trim(),
            Genre = request.Genre,
            ReleaseYear = request.ReleaseYear,
            DurationMinutes = request.DurationMinutes,
        };

        await _movieRepository.AddAsync(movie, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        var response = new MovieDetailsResponse(
            movie.Id,
            movie.Title,
            movie.Director,
            movie.Genre,
            movie.ReleaseYear,
            movie.DurationMinutes,
            0m,
            0);

        return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMovie(
        int id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie is null)
        {
            return NotFound();
        }

        var releaseYearError = _movieValidator.ValidateReleaseYear(request.ReleaseYear);
        if (releaseYearError is not null)
        {
            return ValidationFailure(nameof(request.ReleaseYear), releaseYearError);
        }

        movie.Title = request.Title.Trim();
        movie.Director = request.Director.Trim();
        movie.Genre = request.Genre;
        movie.ReleaseYear = request.ReleaseYear;
        movie.DurationMinutes = request.DurationMinutes;

        await SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMovie(int id, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie is null)
        {
            return NotFound();
        }

        _movieRepository.Remove(movie);
        await SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:int}/reviews")]
    public async Task<ActionResult<ReviewResponse>> CreateReview(
        int id,
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reviewWorkflowService.SubmitAsync(id, request, cancellationToken);
        if (result.MovieNotFound)
        {
            return NotFound();
        }

        if (result.ErrorField is not null)
        {
            return ValidationFailure(result.ErrorField, result.ErrorMessage!);
        }

        if (result.IsDuplicate)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Duplicate review",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status409Conflict,
            });
        }

        return CreatedAtAction(nameof(GetReviews), new { id }, result.Review);
    }

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<IReadOnlyCollection<ReviewResponse>>> GetReviews(
        int id,
        CancellationToken cancellationToken)
    {
        var movieExists = await _movieRepository.ExistsAsync(id, cancellationToken);
        if (!movieExists)
        {
            return NotFound();
        }

        var reviews = await _reviewRepository.Query()
            .AsNoTracking()
            .Where(review => review.MovieId == id)
            .OrderByDescending(review => review.CreatedAt)
            .Select(review => new ReviewResponse(
                review.Id,
                review.MovieId,
                review.UserId,
                review.User.Username,
                review.Score,
                review.Comment,
                review.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(reviews);
    }

    private MovieDetailsResponse MapMovieDetails(MovieAggregate movie) =>
        new(
            movie.Id,
            movie.Title,
            movie.Director,
            movie.Genre,
            movie.ReleaseYear,
            movie.DurationMinutes,
            _ratingCalculator.RoundAverage(movie.AverageScore),
            movie.ReviewCount);

    private static IDictionary<string, string[]> CreateValidationDictionary(string key, string message) =>
        new Dictionary<string, string[]>
        {
            [key] = new[] { message },
        };

    private ActionResult ValidationFailure(string key, string message) =>
        BadRequest(new ValidationProblemDetails(CreateValidationDictionary(key, message)));

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (_movieRepository is not IUnitOfWork unitOfWork)
        {
            throw new InvalidOperationException("Movie repository must implement IUnitOfWork.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private record MovieAggregate(
        int Id,
        string Title,
        string Director,
        Genre Genre,
        int ReleaseYear,
        int DurationMinutes,
        decimal? AverageScore,
        int ReviewCount);
}
