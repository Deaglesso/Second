using System;

namespace Second.Application.Dtos.Requests
{
    public sealed record AddProductImageRequest
    {
        public Guid ProductId { get; init; }

        public string ImageUrl { get; init; } = string.Empty;

        public int Order { get; init; }
    }
}
