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

        public ProductCondition Condition { get; init; }

        public bool IsActive { get; init; }
    }
}
