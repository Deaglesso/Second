using System;

namespace Second.Application.Dtos
{
    public sealed record ProductImageDto
    {
        public Guid Id { get; init; }

        public Guid ProductId { get; init; }

        public string ImageUrl { get; init; } = string.Empty;

        public int Order { get; init; }
    }
}
