namespace Second.Application.Dtos.Requests
{
    public sealed record RefreshTokenRequest
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
