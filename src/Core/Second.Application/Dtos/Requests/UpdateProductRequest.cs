using System;
using Second.Domain.Enums;

namespace Second.Application.Dtos.Requests
{
    public sealed record UpdateProductRequest
    {
        public Guid ProductId { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string? PriceText { get; init; }

        public int Price { get; init; }

        public ProductCondition Condition { get; init; }

        public ProductStatus Status { get; init; }
    }
}
