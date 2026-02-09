using System;
using System.Collections.Generic;
using Second.Domain.Enums;

namespace Second.Application.Dtos.Requests
{
    public sealed record CreateProductRequest
    {
        public Guid SellerUserId { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string? PriceText { get; init; }

        public ProductCondition Condition { get; init; }

        public IReadOnlyList<string> ImageUrls { get; init; } = Array.Empty<string>();
    }
}
