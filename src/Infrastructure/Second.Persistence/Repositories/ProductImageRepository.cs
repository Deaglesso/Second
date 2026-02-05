using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Repositories;
using Second.Domain.Entities;
using Second.Persistence.Data;

namespace Second.Persistence.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductImageRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProductImages
                .AsNoTracking()
                .FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
        }

        public async Task AddAsync(ProductImage image, CancellationToken cancellationToken = default)
        {
            _dbContext.ProductImages.Add(image);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(ProductImage image, CancellationToken cancellationToken = default)
        {
            _dbContext.ProductImages.Remove(image);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
