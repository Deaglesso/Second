using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Services;
using StackExchange.Redis;

namespace Second.Persistence.Implementations.Services
{
    public class RedisTokenRevocationService : ITokenRevocationService
    {
        private const string KeyPrefix = "auth:revoked:jti:";
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisTokenRevocationService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task RevokeJtiAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var ttl = expiresAtUtc - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                ttl = TimeSpan.FromMinutes(1);
            }

            await database.StringSetAsync(KeyPrefix + jti, "1", ttl);
        }

        public async Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
        {
            var database = _connectionMultiplexer.GetDatabase();
            return await database.KeyExistsAsync(KeyPrefix + jti);
        }
    }
}
