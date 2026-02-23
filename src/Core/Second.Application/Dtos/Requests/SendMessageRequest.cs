using System;
using System.Text.Json.Serialization;

namespace Second.Application.Dtos.Requests
{
    public sealed record SendMessageRequest
    {
        [JsonIgnore]
        public Guid ChatRoomId { get; init; }

        [JsonIgnore]
        public Guid SenderId { get; init; }

        public string Content { get; init; } = string.Empty;
    }
}
