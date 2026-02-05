using System;

namespace Second.Application.Dtos.Requests
{
    public sealed record SendMessageRequest
    {
        public Guid ChatRoomId { get; init; }

        public Guid SenderId { get; init; }

        public string Content { get; init; } = string.Empty;
    }
}
