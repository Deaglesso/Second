using System;

namespace Second.Application.Dtos.Requests
{
    public sealed record CreateSellerProfileRequest
    {
        public Guid UserId { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public string? Bio { get; init; }
    }
}
