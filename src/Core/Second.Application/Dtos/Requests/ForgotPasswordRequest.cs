namespace Second.Application.Dtos.Requests
{
    public sealed record ForgotPasswordRequest
    {
        public string Email { get; init; } = string.Empty;
    }
}
