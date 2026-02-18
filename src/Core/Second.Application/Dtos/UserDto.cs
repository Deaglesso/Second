using System;
using Second.Domain.Enums;

namespace Second.Application.Dtos
{
    public sealed record UserDto
    {
        public Guid Id { get; init; }

        public string Email { get; init; } = string.Empty;

        public UserRole Role { get; init; }

        public bool EmailVerified { get; init; }

        public decimal SellerRating { get; init; }

        public int ListingLimit { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
