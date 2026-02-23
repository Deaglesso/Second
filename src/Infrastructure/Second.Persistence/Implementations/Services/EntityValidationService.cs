using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Services;
using Second.Domain.Enums;
using Second.Persistence.Data;

namespace Second.Persistence.Implementations.Services
{
    public class EntityValidationService : IEntityValidationService
    {
        private readonly AppDbContext _dbContext;

        public EntityValidationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> SellerUserExistsAsync(Guid sellerUserId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(user => user.Id == sellerUserId && (user.Role == Domain.Enums.UserRole.Seller || user.Role == Domain.Enums.UserRole.Admin), cancellationToken);
        }

        public async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == productId, cancellationToken);
        }

        public async Task<bool> ProductTitleUniqueForSellerAsync(Guid sellerUserId, string title, Guid? excludedProductId, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.Products.AsNoTracking().AnyAsync(product =>
                product.SellerUserId == sellerUserId &&
                product.Title == title &&
                (!excludedProductId.HasValue || product.Id != excludedProductId.Value), cancellationToken);
        }

        public async Task<bool> SellerHasCapacityForActiveListingAsync(Guid sellerUserId, int maxActiveListings, Guid? excludedProductId, CancellationToken cancellationToken = default)
        {
            var activeListings = await _dbContext.Products.AsNoTracking().CountAsync(product =>
                product.SellerUserId == sellerUserId &&
                product.Status == ProductStatus.Active &&
                (!excludedProductId.HasValue || product.Id != excludedProductId.Value), cancellationToken);

            return activeListings < maxActiveListings;
        }

        public async Task<Guid?> GetSellerUserIdForProductAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products.AsNoTracking()
                .Where(product => product.Id == productId)
                .Select(product => (Guid?)product.SellerUserId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ProductImageOrderUniqueAsync(Guid productId, int order, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.ProductImages.AsNoTracking()
                .AnyAsync(image => image.ProductId == productId && image.Order == order, cancellationToken);
        }

        public async Task<bool> ProductHasImageCapacityAsync(Guid productId, int maxImages, CancellationToken cancellationToken = default)
        {
            var currentCount = await _dbContext.ProductImages.AsNoTracking()
                .CountAsync(image => image.ProductId == productId, cancellationToken);

            return currentCount < maxImages;
        }

        public async Task<bool> ProductIsActiveAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products.AsNoTracking()
                .AnyAsync(product => product.Id == productId && product.Status == ProductStatus.Active, cancellationToken);
        }

        public async Task<bool> SellerMatchesProductAsync(Guid productId, Guid sellerUserId, CancellationToken cancellationToken = default)
        {
            var ownerId = await _dbContext.Products.AsNoTracking()
                .Where(product => product.Id == productId)
                .Select(product => product.SellerUserId)
                .FirstOrDefaultAsync(cancellationToken);

            return ownerId != Guid.Empty && ownerId == sellerUserId;
        }

        public async Task<bool> ChatRoomExistsAsync(Guid chatRoomId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms.AsNoTracking().AnyAsync(chatRoom => chatRoom.Id == chatRoomId, cancellationToken);
        }

        public async Task<bool> ChatRoomExistsForParticipantsAsync(Guid productId, Guid buyerId, Guid sellerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms.AsNoTracking().AnyAsync(chatRoom =>
                chatRoom.ProductId == productId && chatRoom.BuyerId == buyerId && chatRoom.SellerId == sellerId, cancellationToken);
        }

        public async Task<bool> ChatRoomHasParticipantAsync(Guid chatRoomId, Guid participantId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms.AsNoTracking().AnyAsync(chatRoom =>
                chatRoom.Id == chatRoomId && (chatRoom.BuyerId == participantId || chatRoom.SellerId == participantId), cancellationToken);
        }

        public async Task<bool> ReportTargetExistsAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
        {
            return targetType switch
            {
                ReportTargetType.Product => await _dbContext.Products.AsNoTracking().AnyAsync(product => product.Id == targetId, cancellationToken),
                ReportTargetType.Seller => await _dbContext.Users.AsNoTracking().AnyAsync(user => user.Id == targetId, cancellationToken),
                ReportTargetType.Message => await _dbContext.Messages.AsNoTracking().AnyAsync(message => message.Id == targetId, cancellationToken),
                _ => false
            };
        }

        public async Task<bool> ReportIsUniqueAsync(Guid reporterId, ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.Reports.AsNoTracking().AnyAsync(report =>
                report.ReporterId == reporterId && report.TargetType == targetType && report.TargetId == targetId, cancellationToken);
        }
    }
}
