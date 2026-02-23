using Second.Domain.Enums;

namespace Second.Application.Dtos.Requests
{
    public sealed record GetActiveProductsRequest
    {
        public string? Query { get; init; }

        public ProductCondition? Condition { get; init; }

        public int? MinPrice { get; init; }

        public int? MaxPrice { get; init; }

        public string SortBy { get; init; } = "newest";
    }
}
