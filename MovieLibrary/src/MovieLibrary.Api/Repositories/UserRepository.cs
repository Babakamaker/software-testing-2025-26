using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Data;
using MovieLibrary.Api.Domain;

namespace MovieLibrary.Api.Repositories;

public class UserRepository(MovieLibraryDbContext dbContext) : IUserRepository
{
    public IQueryable<User> Query() => dbContext.Users;

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
}
