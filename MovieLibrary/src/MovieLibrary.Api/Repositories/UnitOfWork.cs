using MovieLibrary.Api.Data;

namespace MovieLibrary.Api.Repositories;

public class UnitOfWork(MovieLibraryDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
