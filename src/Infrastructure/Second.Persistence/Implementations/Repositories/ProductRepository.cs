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
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .Include(product => product.Images)
                .Include(product => product.SellerProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetBySellerProfileIdAsync(Guid sellerProfileId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .Include(product => product.Images)
                .AsNoTracking()
                .Where(product => product.SellerProfileId == sellerProfileId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .Include(product => product.Images)
                .AsNoTracking()
                .Where(product => product.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
