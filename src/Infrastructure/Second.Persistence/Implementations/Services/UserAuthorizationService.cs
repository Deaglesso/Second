using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;

namespace Second.Persistence.Implementations.Services
{
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly IUserRepository _userRepository;

        public UserAuthorizationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> IsSellerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken: cancellationToken);
            return user is not null && user.IsSeller();
        }
    }
}
