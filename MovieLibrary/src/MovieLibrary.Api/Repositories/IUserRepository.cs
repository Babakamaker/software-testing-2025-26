using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Repositories;

public interface IUserRepository
{
    IQueryable<User> Query();

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
