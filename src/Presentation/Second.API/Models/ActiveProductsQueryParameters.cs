using Second.Domain.Enums;

namespace Second.API.Models
{
    public sealed record ActiveProductsQueryParameters
    {
        public string? Q { get; init; }

        public ProductCondition? Condition { get; init; }

        public int? MinPrice { get; init; }

        public int? MaxPrice { get; init; }

        public string SortBy { get; init; } = "newest";
    }
}
