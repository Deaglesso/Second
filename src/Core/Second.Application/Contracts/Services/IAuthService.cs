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

        Task<UserDto?> GetUserByIdAsync(System.Guid userId, CancellationToken cancellationToken = default);
    }
}
