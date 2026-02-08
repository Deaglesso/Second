using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos.Requests;
using Second.Domain.Entities;
using Second.Domain.Enums;
using Second.Persistence.Implementations.Services;
using Xunit;

namespace Second.Persistence.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegisterAsync_Throws_WhenEmailAlreadyExists()
        {
            var userRepository = new Mock<IUserRepository>();
            var tokenService = new Mock<ITokenService>();
            var passwordHasher = new Mock<IPasswordHasher<User>>();
            var service = new AuthService(userRepository.Object, tokenService.Object, passwordHasher.Object);

            var request = new RegisterUserRequest
            {
                Email = "existing@example.com",
                Password = "Password1",
                Role = UserRole.User
            };

            userRepository
                .Setup(repository => repository.GetByEmailAsync("existing@example.com", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Email = "existing@example.com" });

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task LoginAsync_Throws_WhenPasswordInvalid()
        {
            var userRepository = new Mock<IUserRepository>();
            var tokenService = new Mock<ITokenService>();
            var passwordHasher = new Mock<IPasswordHasher<User>>();
            var service = new AuthService(userRepository.Object, tokenService.Object, passwordHasher.Object);

            var request = new LoginRequest
            {
                Email = "user@example.com",
                Password = "bad-password"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = "hashed"
            };

            userRepository
                .Setup(repository => repository.GetByEmailAsync("user@example.com", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            passwordHasher
                .Setup(hasher => hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password))
                .Returns(PasswordVerificationResult.Failed);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task RegisterAsync_ReturnsTokenAndPersistsUser_WhenValid()
        {
            var userRepository = new Mock<IUserRepository>();
            var tokenService = new Mock<ITokenService>();
            var passwordHasher = new Mock<IPasswordHasher<User>>();
            var service = new AuthService(userRepository.Object, tokenService.Object, passwordHasher.Object);

            var request = new RegisterUserRequest
            {
                Email = "seller@example.com",
                Password = "Password1",
                Role = UserRole.Seller
            };

            userRepository
                .Setup(repository => repository.GetByEmailAsync("seller@example.com", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            passwordHasher
                .Setup(hasher => hasher.HashPassword(It.IsAny<User>(), request.Password))
                .Returns("hashed-password");

            tokenService
                .Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

            User? persistedUser = null;
            userRepository
                .Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((user, _) => persistedUser = user)
                .Returns(Task.CompletedTask);

            var result = await service.RegisterAsync(request, CancellationToken.None);

            Assert.NotNull(persistedUser);
            Assert.Equal("seller@example.com", persistedUser!.Email);
            Assert.Equal(UserRole.Seller, persistedUser.Role);
            Assert.Equal("jwt-token", result.AccessToken);
            Assert.Equal(UserRole.Seller, result.Role);
        }
    }
}
