using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Repositories;

public interface IReviewRepository
{
    IQueryable<Review> Query();

    Task<bool> ExistsForMovieAndUserAsync(int movieId, int userId, CancellationToken cancellationToken);

    Task AddAsync(Review review, CancellationToken cancellationToken);
}
