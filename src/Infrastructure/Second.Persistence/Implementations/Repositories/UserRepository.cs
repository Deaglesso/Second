using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Repositories;
using Second.Domain.Entities;
using Second.Persistence.Data;

namespace Second.Persistence.Implementations.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var query = includeDeleted ? _dbContext.Users.IgnoreQueryFilters() : _dbContext.Users;
            return await query.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var query = includeDeleted ? _dbContext.Users.IgnoreQueryFilters() : _dbContext.Users;
            return await query.FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetDeletedUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(user => user.IsDeleted)
                .OrderByDescending(user => user.DeletedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
