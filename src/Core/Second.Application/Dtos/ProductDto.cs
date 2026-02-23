using System;
using System.Collections.Generic;
using Second.Domain.Enums;

namespace Second.Application.Dtos
{
    public sealed record ProductDto
    {
        public Guid Id { get; init; }

        public Guid SellerUserId { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string? PriceText { get; init; }

        public int Price { get; init; }

        public ProductCondition Condition { get; init; }

        public ProductStatus Status { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public IReadOnlyList<ProductImageDto> Images { get; init; } = Array.Empty<ProductImageDto>();
    }
}
