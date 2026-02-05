using System;
using Second.Domain.Entities.Common;

namespace Second.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid ChatRoomId { get; set; }

        public Guid SenderId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public ChatRoom? ChatRoom { get; set; }
    }
}
