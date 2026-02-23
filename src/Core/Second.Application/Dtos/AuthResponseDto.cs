using System;
using Second.Domain.Enums;

namespace Second.Application.Dtos
{
    public sealed record AuthResponseDto
    {
        public Guid UserId { get; init; }

        public string Email { get; init; } = string.Empty;

        public UserRole Role { get; init; }

        public string AccessToken { get; init; } = string.Empty;

        public DateTime ExpiresAtUtc { get; init; }

        public string RefreshToken { get; init; } = string.Empty;

        public DateTime RefreshTokenExpiresAtUtc { get; init; }
    }
}
