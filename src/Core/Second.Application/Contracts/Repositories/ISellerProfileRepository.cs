using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface ISellerProfileRepository
    {
        Task<SellerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<SellerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task AddAsync(SellerProfile profile, CancellationToken cancellationToken = default);

        Task UpdateAsync(SellerProfile profile, CancellationToken cancellationToken = default);
    }
}
