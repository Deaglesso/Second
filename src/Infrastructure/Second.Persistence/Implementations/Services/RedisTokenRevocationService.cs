using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Second.Application.Contracts.Services;
using StackExchange.Redis;

namespace Second.Persistence.Implementations.Services
{
    public class RedisTokenRevocationService : ITokenRevocationService
    {
        private const string KeyPrefix = "auth:revoked:jti:";
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RedisTokenRevocationService> _logger;

        public RedisTokenRevocationService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisTokenRevocationService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
        }

        public async Task RevokeJtiAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var ttl = expiresAtUtc - DateTime.UtcNow;
                if (ttl <= TimeSpan.Zero)
                {
                    ttl = TimeSpan.FromMinutes(1);
                }

                await database.StringSetAsync(KeyPrefix + jti, "1", ttl);
            }
            catch (RedisException exception)
            {
                _logger.LogWarning(exception, "Failed to revoke token jti {Jti} because Redis is unavailable.", jti);
            }
        }

        public async Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                return await database.KeyExistsAsync(KeyPrefix + jti);
            }
            catch (RedisException exception)
            {
                _logger.LogWarning(exception, "Failed to check token revocation for jti {Jti}; treating token as not revoked.", jti);
                return false;
            }
        }
    }
}
