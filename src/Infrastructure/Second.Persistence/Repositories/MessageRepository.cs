using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Repositories;
using Second.Domain.Entities;
using Second.Persistence.Data;

namespace Second.Persistence.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _dbContext;

        public MessageRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Message>> GetByChatRoomIdAsync(Guid chatRoomId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Messages
                .AsNoTracking()
                .Where(message => message.ChatRoomId == chatRoomId)
                .OrderBy(message => message.SentAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
