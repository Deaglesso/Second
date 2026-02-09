using System;
using System.Threading;
using System.Threading.Tasks;

namespace Second.Application.Contracts.Services
{
    public interface ITokenRevocationService
    {
        Task RevokeJtiAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken = default);

        Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default);
    }
}
