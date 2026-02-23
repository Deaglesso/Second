using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(string email, bool includeDeleted = false, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<User>> GetDeletedUsersAsync(CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
