using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Repositories;

public interface IMovieRepository
{
    IQueryable<Movie> Query();

    Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Movie?> GetByIdWithReviewsAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Movie movie, CancellationToken cancellationToken);

    void Remove(Movie movie);
}
