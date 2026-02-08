using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
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
                Role = request.Role,
                EmailVerified = false
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user, cancellationToken);

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
    }
}
