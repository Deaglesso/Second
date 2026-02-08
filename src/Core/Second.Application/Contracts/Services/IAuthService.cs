using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;

namespace Second.Application.Contracts.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

        Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<AuthResponseDto> BecomeSellerAsync(Guid userId, CancellationToken cancellationToken = default);

        Task LogoutAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken = default);

        Task RequestEmailVerificationAsync(RequestEmailVerificationRequest request, CancellationToken cancellationToken = default);

        Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);

        Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

        Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    }
}
