using Microsoft.EntityFrameworkCore;
using OrionPOS.Application.Auth;
using OrionPOS.Domain.Auth;
using OrionPOS.Infra.Persistence;

namespace OrionPOS.Infra.Repositories;

public sealed class UserRepository(OrionPosDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
