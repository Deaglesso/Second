using System;
using System.Collections.Generic;
using Second.Domain.Entities.Common;
using Second.Domain.Enums;

namespace Second.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid SellerProfileId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? PriceText { get; set; }

        public ProductCondition Condition { get; set; }

        public bool IsActive { get; set; }

        public SellerProfile? SellerProfile { get; set; }

        public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
