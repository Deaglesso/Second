using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Services;

namespace Second.Persistence.Implementations.Services
{
    public class NoOpTokenRevocationService : ITokenRevocationService
    {
        public Task RevokeJtiAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}
