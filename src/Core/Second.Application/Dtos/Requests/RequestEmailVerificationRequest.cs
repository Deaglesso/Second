namespace Second.Application.Dtos.Requests
{
    public sealed record RequestEmailVerificationRequest
    {
        public string Email { get; init; } = string.Empty;
    }
}
