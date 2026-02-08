using System;
using Second.Domain.Enums;

namespace Second.Application.Dtos
{
    public sealed record SellerProfileDto
    {
        public Guid Id { get; init; }

        public Guid? UserId { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public string? Bio { get; init; }

        public SellerStatus Status { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
