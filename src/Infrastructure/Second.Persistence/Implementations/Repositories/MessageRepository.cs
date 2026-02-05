using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Repositories;
using Second.Domain.Entities;
using Second.Persistence.Data;

namespace Second.Persistence.Implementations.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _dbContext;

        public MessageRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(IReadOnlyList<Message> Items, int TotalCount)> GetByChatRoomIdAsync(
            Guid chatRoomId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Messages
                .AsNoTracking()
                .Where(message => message.ChatRoomId == chatRoomId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(message => message.SentAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
