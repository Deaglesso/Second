namespace Second.Application.Dtos.Requests
{
    public sealed record VerifyEmailRequest
    {
        public string Token { get; init; } = string.Empty;
    }
}
