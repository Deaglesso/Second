using System;

namespace Second.Application.Dtos.Requests
{
    public sealed record StartChatRequest
    {
        public Guid ProductId { get; init; }

        public Guid BuyerId { get; init; }

        public Guid SellerId { get; init; }
    }
}
