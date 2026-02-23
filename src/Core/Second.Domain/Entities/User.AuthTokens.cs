using System;

namespace Second.Domain.Entities
{
    public partial class User
    {
        public string? EmailVerificationTokenHash { get; set; }

        public DateTime? EmailVerificationTokenExpiresAtUtc { get; set; }

        public string? PasswordResetTokenHash { get; set; }

        public DateTime? PasswordResetTokenExpiresAtUtc { get; set; }

        public string? RefreshTokenHash { get; set; }

        public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    }
}
