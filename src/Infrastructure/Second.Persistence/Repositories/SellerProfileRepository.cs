using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Repositories;
using Second.Domain.Entities;
using Second.Persistence.Data;

namespace Second.Persistence.Repositories
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
