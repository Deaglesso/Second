namespace Second.Persistence.Configuration
{
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";

        public bool Enabled { get; init; }

        public string FromAddress { get; init; } = string.Empty;

        public string FromName { get; init; } = "Second";

        public string SmtpHost { get; init; } = string.Empty;

        public int SmtpPort { get; init; } = 587;

        public bool UseSsl { get; init; } = true;

        public bool UseDefaultCredentials { get; init; }

        public string? Username { get; init; }

        public string? Password { get; init; }

        public int TimeoutMilliseconds { get; init; } = 10000;
    }
}
