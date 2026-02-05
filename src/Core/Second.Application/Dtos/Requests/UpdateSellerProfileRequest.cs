using System;
using Second.Domain.Enums;

namespace Second.Application.Dtos.Requests
{
    public sealed record UpdateSellerProfileRequest
    {
        public Guid SellerProfileId { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public string? Bio { get; init; }

        public SellerStatus Status { get; init; }
    }
}
