using System;
using System.Collections.Generic;
using Second.Domain.Entities.Common;

namespace Second.Domain.Entities
{
    public class ChatRoom : BaseEntity
    {
        public Guid ProductId { get; set; }

        public Guid BuyerId { get; set; }

        public Guid SellerId { get; set; }

        public Product? Product { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
