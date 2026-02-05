using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface IMessageRepository
    {
        Task<(IReadOnlyList<Message> Items, int TotalCount)> GetByChatRoomIdAsync(
            Guid chatRoomId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task AddAsync(Message message, CancellationToken cancellationToken = default);
    }
}
