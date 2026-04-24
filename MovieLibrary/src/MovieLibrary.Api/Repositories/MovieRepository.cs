using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Data;
using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Repositories;

public class MovieRepository(MovieLibraryDbContext dbContext) : IMovieRepository, IUnitOfWork
{
    public IQueryable<Movie> Query() => dbContext.Movies;

    public Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Movies.SingleOrDefaultAsync(movie => movie.Id == id, cancellationToken);

    public Task<Movie?> GetByIdWithReviewsAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Movies
            .Include(movie => movie.Reviews)
            .SingleOrDefaultAsync(movie => movie.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Movies.AnyAsync(movie => movie.Id == id, cancellationToken);

    public Task AddAsync(Movie movie, CancellationToken cancellationToken) =>
        dbContext.Movies.AddAsync(movie, cancellationToken).AsTask();

    public void Remove(Movie movie) => dbContext.Movies.Remove(movie);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
