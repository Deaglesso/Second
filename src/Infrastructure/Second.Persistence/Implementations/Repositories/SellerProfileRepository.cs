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
    public class SellerProfileRepository : ISellerProfileRepository
    {
        private readonly AppDbContext _dbContext;

        public SellerProfileRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SellerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.SellerProfiles
                .Include(profile => profile.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.Id == id, cancellationToken);
        }

        public async Task<SellerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.SellerProfiles
                .Include(profile => profile.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
        }

        public async Task<(IReadOnlyList<SellerProfile> Items, int TotalCount)> GetAllAsync(
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.SellerProfiles
                .AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(profile => profile.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(SellerProfile profile, CancellationToken cancellationToken = default)
        {
            _dbContext.SellerProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(SellerProfile profile, CancellationToken cancellationToken = default)
        {
            _dbContext.SellerProfiles.Update(profile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
