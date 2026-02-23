using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Repositories;
using Second.Application.Dtos.Requests;
using Second.Domain.Entities;
using Second.Domain.Enums;
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
                .Include(product => product.SellerUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetBySellerUserIdAsync(
            Guid sellerUserId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Products
                .Include(product => product.Images)
                .AsNoTracking()
                .Where(product => product.SellerUserId == sellerUserId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(product => product.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetActiveAsync(
            GetActiveProductsRequest request,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Products
                .Include(product => product.Images)
                .AsNoTracking()
                .Where(product => product.Status == ProductStatus.Active);

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var normalizedQuery = request.Query.Trim();
                query = query.Where(product =>
                    product.Title.Contains(normalizedQuery) ||
                    product.Description.Contains(normalizedQuery));
            }

            if (request.Condition.HasValue)
            {
                query = query.Where(product => product.Condition == request.Condition.Value);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(product => product.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(product => product.Price <= request.MaxPrice.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = request.SortBy.ToLowerInvariant() switch
            {
                "price_asc" => query.OrderBy(product => product.Price).ThenByDescending(product => product.CreatedAt),
                "price_desc" => query.OrderByDescending(product => product.Price).ThenByDescending(product => product.CreatedAt),
                _ => query.OrderByDescending(product => product.CreatedAt)
            };

            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
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
