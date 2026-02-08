using System.Collections.Generic;
using Second.Domain.Entities.Common;
using Second.Domain.Enums;

namespace Second.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public bool EmailVerified { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public bool IsSeller()
        {
            return Role == UserRole.Seller || Role == UserRole.Admin;
        }
    }
}
