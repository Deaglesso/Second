using System;
using Second.Domain.Entities.Common;

namespace Second.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int Order { get; set; }

        public Product? Product { get; set; }
    }
}
