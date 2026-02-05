using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface IProductImageRepository
    {
        Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task AddAsync(ProductImage image, CancellationToken cancellationToken = default);

        Task RemoveAsync(ProductImage image, CancellationToken cancellationToken = default);
    }
}
