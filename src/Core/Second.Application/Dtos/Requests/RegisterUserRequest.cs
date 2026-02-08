using Second.Domain.Enums;

namespace Second.Application.Dtos.Requests
{
    public sealed record RegisterUserRequest
    {
        public string Email { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;

        public UserRole Role { get; init; } = UserRole.User;
    }
}
