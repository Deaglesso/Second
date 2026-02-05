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
    public class ChatRoomRepository : IChatRoomRepository
    {
        private readonly AppDbContext _dbContext;

        public ChatRoomRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms
                .Include(chatRoom => chatRoom.Messages)
                .AsNoTracking()
                .FirstOrDefaultAsync(chatRoom => chatRoom.Id == id, cancellationToken);
        }

        public async Task<ChatRoom?> GetByProductAndUsersAsync(Guid productId, Guid buyerId, Guid sellerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms
                .Include(chatRoom => chatRoom.Messages)
                .AsNoTracking()
                .FirstOrDefaultAsync(chatRoom =>
                    chatRoom.ProductId == productId &&
                    chatRoom.BuyerId == buyerId &&
                    chatRoom.SellerId == sellerId,
                    cancellationToken);
        }

        public async Task<(IReadOnlyList<ChatRoom> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.ChatRooms
                .Include(chatRoom => chatRoom.Product)
                .AsNoTracking()
                .Where(chatRoom => chatRoom.BuyerId == userId || chatRoom.SellerId == userId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(chatRoom => chatRoom.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(ChatRoom chatRoom, CancellationToken cancellationToken = default)
        {
            _dbContext.ChatRooms.Add(chatRoom);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
