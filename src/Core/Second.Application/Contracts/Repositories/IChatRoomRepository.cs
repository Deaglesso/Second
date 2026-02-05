using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface IChatRoomRepository
    {
        Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ChatRoom?> GetByProductAndUsersAsync(Guid productId, Guid buyerId, Guid sellerId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ChatRoom>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task AddAsync(ChatRoom chatRoom, CancellationToken cancellationToken = default);
    }
}
