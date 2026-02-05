using System;

namespace Second.Application.Dtos
{
    public sealed record ChatRoomDto
    {
        public Guid Id { get; init; }

        public Guid ProductId { get; init; }

        public Guid BuyerId { get; init; }

        public Guid SellerId { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
