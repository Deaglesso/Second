using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Domain.Entities;

namespace Second.Persistence.Implementations.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenRevocationService _tokenRevocationService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,
            ITokenRevocationService tokenRevocationService,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _tokenRevocationService = tokenRevocationService;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, includeDeleted: true, cancellationToken);
            if (existingUser is not null)
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            var user = new User
            {
                Email = normalizedEmail,
                Role = Domain.Enums.UserRole.User,
                EmailVerified = false
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user, cancellationToken);

            await RequestEmailVerificationAsync(new RequestEmailVerificationRequest { Email = user.Email }, cancellationToken);

            var (token, expiresAtUtc) = _tokenService.GenerateToken(user);
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                AccessToken = token,
                ExpiresAtUtc = expiresAtUtc
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken: cancellationToken);

            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var (token, expiresAtUtc) = _tokenService.GenerateToken(user);
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                AccessToken = token,
                ExpiresAtUtc = expiresAtUtc
            };
        }

        public async Task<AuthResponseDto> BecomeSellerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken: cancellationToken);
            if (user is null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (user.Role == Domain.Enums.UserRole.User)
            {
                user.Role = Domain.Enums.UserRole.Seller;
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            var (token, expiresAtUtc) = _tokenService.GenerateToken(user);
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                AccessToken = token,
                ExpiresAtUtc = expiresAtUtc
            };
        }

        public Task LogoutAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            return _tokenRevocationService.RevokeJtiAsync(jti, expiresAtUtc, cancellationToken);
        }

        public async Task RequestEmailVerificationAsync(RequestEmailVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken: cancellationToken);
            if (user is null || user.EmailVerified)
            {
                return;
            }

            var rawToken = GenerateOpaqueToken();
            user.EmailVerificationTokenHash = ComputeHash(rawToken);
            user.EmailVerificationTokenExpiresAtUtc = DateTime.UtcNow.AddHours(1);
            await _userRepository.UpdateAsync(user, cancellationToken);

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var verificationUrl = $"{frontendBaseUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(rawToken)}";
            await _emailSender.SendAsync(
                user.Email,
                "Verify your email",
                $"Please verify your email by opening this link: {verificationUrl}",
                cancellationToken);
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
        {
            var tokenHash = ComputeHash(request.Token);
            var user = await _userRepository.GetByEmailVerificationTokenHashAsync(tokenHash, cancellationToken);
            if (user is null)
            {
                return false;
            }

            if (!user.EmailVerificationTokenExpiresAtUtc.HasValue || user.EmailVerificationTokenExpiresAtUtc.Value < DateTime.UtcNow)
            {
                return false;
            }

            user.EmailVerified = true;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiresAtUtc = null;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken: cancellationToken);
            if (user is null)
            {
                return;
            }

            var rawToken = GenerateOpaqueToken();
            user.PasswordResetTokenHash = ComputeHash(rawToken);
            user.PasswordResetTokenExpiresAtUtc = DateTime.UtcNow.AddHours(1);
            await _userRepository.UpdateAsync(user, cancellationToken);

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var resetUrl = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";
            await _emailSender.SendAsync(
                user.Email,
                "Reset your password",
                $"Reset your password by opening this link: {resetUrl}",
                cancellationToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var tokenHash = ComputeHash(request.Token);
            var user = await _userRepository.GetByPasswordResetTokenHashAsync(tokenHash, cancellationToken);
            if (user is null || !user.PasswordResetTokenExpiresAtUtc.HasValue || user.PasswordResetTokenExpiresAtUtc.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("The reset token is invalid or expired.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAtUtc = null;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken: cancellationToken);
            if (user is null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt
            };
        }

        private static string GenerateOpaqueToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string ComputeHash(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }
    }
}
