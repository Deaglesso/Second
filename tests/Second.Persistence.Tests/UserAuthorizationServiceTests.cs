using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Second.Application.Contracts.Repositories;
using Second.Domain.Entities;
using Second.Domain.Enums;
using Second.Persistence.Implementations.Services;
using Xunit;

namespace Second.Persistence.Tests
{
    public class UserAuthorizationServiceTests
    {
        [Fact]
        public async Task IsSellerAsync_ReturnsTrue_ForSellerRole()
        {
            var repository = new Mock<IUserRepository>();
            var service = new UserAuthorizationService(repository.Object);
            var userId = Guid.NewGuid();

            repository
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = userId, Role = UserRole.Seller });

            var result = await service.IsSellerAsync(userId, CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task IsSellerAsync_ReturnsFalse_ForRegularUser()
        {
            var repository = new Mock<IUserRepository>();
            var service = new UserAuthorizationService(repository.Object);
            var userId = Guid.NewGuid();

            repository
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = userId, Role = UserRole.User });

            var result = await service.IsSellerAsync(userId, CancellationToken.None);

            Assert.False(result);
        }
    }
}
