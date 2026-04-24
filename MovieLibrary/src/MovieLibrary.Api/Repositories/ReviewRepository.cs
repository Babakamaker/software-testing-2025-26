using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Data;
using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Repositories;

public class ReviewRepository(MovieLibraryDbContext dbContext) : IReviewRepository
{
    public IQueryable<Review> Query() => dbContext.Reviews;

    public Task<bool> ExistsForMovieAndUserAsync(int movieId, int userId, CancellationToken cancellationToken) =>
        dbContext.Reviews.AnyAsync(review => review.MovieId == movieId && review.UserId == userId, cancellationToken);

    public Task AddAsync(Review review, CancellationToken cancellationToken) =>
        dbContext.Reviews.AddAsync(review, cancellationToken).AsTask();
}
