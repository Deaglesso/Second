using System;

namespace Second.Domain.Entities.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public void SoftDelete(DateTime utcNow)
        {
            IsDeleted = true;
            DeletedAt = utcNow;
            UpdatedAt = utcNow;
        }

        public void Restore(DateTime utcNow)
        {
            IsDeleted = false;
            DeletedAt = null;
            UpdatedAt = utcNow;
        }
    }
}
