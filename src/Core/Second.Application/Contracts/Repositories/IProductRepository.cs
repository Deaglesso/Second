using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Product>> GetBySellerProfileIdAsync(Guid sellerProfileId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default);

        Task AddAsync(Product product, CancellationToken cancellationToken = default);

        Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    }
}
