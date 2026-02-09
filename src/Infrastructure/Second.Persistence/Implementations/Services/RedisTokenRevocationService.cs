using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Second.Application.Contracts.Services;
using StackExchange.Redis;

namespace Second.Persistence.Implementations.Services
{
    public class RedisTokenRevocationService : ITokenRevocationService
    {
        private const string KeyPrefix = "auth:revoked:jti:";
        private readonly string _redisConnectionString;
        private readonly ILogger<RedisTokenRevocationService> _logger;
        private IConnectionMultiplexer? _connectionMultiplexer;

        public RedisTokenRevocationService(IConfiguration configuration, ILogger<RedisTokenRevocationService> logger)
        {
            _redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379,abortConnect=false";
            _logger = logger;
        }

        public async Task RevokeJtiAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            try
            {
                var connection = await GetConnectionAsync();
                if (connection is null)
                {
                    return;
                }

                var database = connection.GetDatabase();
                var ttl = expiresAtUtc - DateTime.UtcNow;
                if (ttl <= TimeSpan.Zero)
                {
                    ttl = TimeSpan.FromMinutes(1);
                }

                await database.StringSetAsync(KeyPrefix + jti, "1", ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke JWT jti in Redis. Continuing without hard failure.");
            }
        }

        public async Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
        {
            try
            {
                var connection = await GetConnectionAsync();
                if (connection is null)
                {
                    return false;
                }

                var database = connection.GetDatabase();
                return await database.KeyExistsAsync(KeyPrefix + jti);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check JWT revocation in Redis. Treating token as not revoked.");
                return false;
            }
        }

        private async Task<IConnectionMultiplexer?> GetConnectionAsync()
        {
            if (_connectionMultiplexer is not null)
            {
                return _connectionMultiplexer;
            }

            try
            {
                var options = ConfigurationOptions.Parse(_redisConnectionString);
                options.AbortOnConnectFail = false;
                _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(options);
                return _connectionMultiplexer;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis unavailable for token revocation. Falling back to no-op behavior.");
                return null;
            }
        }
    }
}
