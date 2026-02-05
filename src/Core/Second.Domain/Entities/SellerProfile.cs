using System;
using System.Collections.Generic;
using Second.Domain.Entities.Common;
using Second.Domain.Enums;

namespace Second.Domain.Entities
{
    public class SellerProfile : BaseEntity
    {
        public Guid UserId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public SellerStatus Status { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
