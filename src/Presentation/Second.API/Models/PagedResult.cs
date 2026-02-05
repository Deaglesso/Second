using System;
using System.Collections.Generic;

namespace Second.API.Models
{
    public sealed record PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public int TotalCount { get; init; }

        public int TotalPages { get; init; }
    }
}
