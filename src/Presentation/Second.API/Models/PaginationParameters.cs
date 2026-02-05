namespace Second.API.Models
{
    public sealed record PaginationParameters
    {
        public const int MaxPageSize = 100;

        public int PageNumber { get; init; } = 1;

        public int PageSize { get; init; } = 20;

        public bool IsValid()
        {
            return PageNumber >= 1 && PageSize >= 1 && PageSize <= MaxPageSize;
        }

        public int Skip => (PageNumber - 1) * PageSize;
    }
}
