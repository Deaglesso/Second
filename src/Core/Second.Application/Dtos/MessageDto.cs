using System;

namespace Second.Application.Dtos
{
    public sealed record MessageDto
    {
        public Guid Id { get; init; }

        public Guid ChatRoomId { get; init; }

        public Guid SenderId { get; init; }

        public string Content { get; init; } = string.Empty;

        public DateTime SentAt { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
