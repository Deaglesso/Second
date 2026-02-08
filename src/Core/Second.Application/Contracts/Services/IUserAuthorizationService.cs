using System;
using System.Threading;
using System.Threading.Tasks;

namespace Second.Application.Contracts.Services
{
    public interface IUserAuthorizationService
    {
        Task<bool> IsSellerAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
