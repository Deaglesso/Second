using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Repositories
{
    public interface IMessageRepository
    {
        Task<IReadOnlyList<Message>> GetByChatRoomIdAsync(Guid chatRoomId, CancellationToken cancellationToken = default);

        Task AddAsync(Message message, CancellationToken cancellationToken = default);
    }
}
