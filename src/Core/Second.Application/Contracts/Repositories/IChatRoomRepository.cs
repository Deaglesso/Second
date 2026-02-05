using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface IChatRoomRepository
    {
        Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ChatRoom?> GetByProductAndUsersAsync(Guid productId, Guid buyerId, Guid sellerId, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<ChatRoom> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task AddAsync(ChatRoom chatRoom, CancellationToken cancellationToken = default);
    }
}
