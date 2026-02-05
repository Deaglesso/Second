using System;
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

        public async Task<bool> SellerProfileExistsAsync(Guid sellerProfileId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.SellerProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.Id == sellerProfileId, cancellationToken);
        }

        public async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == productId, cancellationToken);
        }

        public async Task<bool> ProductTitleUniqueForSellerAsync(Guid sellerProfileId, string title, Guid? excludedProductId, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product =>
                    product.SellerProfileId == sellerProfileId &&
                    product.Title == title &&
                    (!excludedProductId.HasValue || product.Id != excludedProductId.Value),
                    cancellationToken);
        }

        public async Task<bool> SellerHasCapacityForActiveListingAsync(Guid sellerProfileId, int maxActiveListings, Guid? excludedProductId, CancellationToken cancellationToken = default)
        {
            var activeListings = await _dbContext.Products
                .AsNoTracking()
                .CountAsync(product =>
                    product.SellerProfileId == sellerProfileId &&
                    product.IsActive &&
                    (!excludedProductId.HasValue || product.Id != excludedProductId.Value),
                    cancellationToken);

            return activeListings < maxActiveListings;
        }

        public async Task<Guid?> GetSellerProfileIdForProductAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var sellerProfileId = await _dbContext.Products
                .AsNoTracking()
                .Where(product => product.Id == productId)
                .Select(product => (Guid?)product.SellerProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            return sellerProfileId;
        }

        public async Task<bool> ProductImageOrderUniqueAsync(Guid productId, int order, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.ProductImages
                .AsNoTracking()
                .AnyAsync(image =>
                    image.ProductId == productId &&
                    image.Order == order,
                    cancellationToken);
        }

        public async Task<bool> ProductHasImageCapacityAsync(Guid productId, int maxImages, CancellationToken cancellationToken = default)
        {
            var currentCount = await _dbContext.ProductImages
                .AsNoTracking()
                .CountAsync(image => image.ProductId == productId, cancellationToken);

            return currentCount < maxImages;
        }

        public async Task<bool> SellerUserAvailableAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.SellerProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.UserId == userId, cancellationToken);
        }

        public async Task<bool> SellerDisplayNameUniqueAsync(string displayName, Guid? excludedSellerProfileId, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.SellerProfiles
                .AsNoTracking()
                .AnyAsync(profile =>
                    profile.DisplayName == displayName &&
                    (!excludedSellerProfileId.HasValue || profile.Id != excludedSellerProfileId.Value),
                    cancellationToken);
        }

        public async Task<bool> ProductIsActiveAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == productId && product.IsActive, cancellationToken);
        }

        public async Task<bool> SellerMatchesProductAsync(Guid productId, Guid sellerUserId, CancellationToken cancellationToken = default)
        {
            var ownerId = await _dbContext.Products
                .AsNoTracking()
                .Where(product => product.Id == productId)
                .Select(product => product.SellerProfile != null ? product.SellerProfile.UserId : Guid.Empty)
                .FirstOrDefaultAsync(cancellationToken);

            return ownerId != Guid.Empty && ownerId == sellerUserId;
        }

        public async Task<bool> ChatRoomExistsAsync(Guid chatRoomId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms
                .AsNoTracking()
                .AnyAsync(chatRoom => chatRoom.Id == chatRoomId, cancellationToken);
        }

        public async Task<bool> ChatRoomExistsForParticipantsAsync(Guid productId, Guid buyerId, Guid sellerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms
                .AsNoTracking()
                .AnyAsync(chatRoom =>
                    chatRoom.ProductId == productId &&
                    chatRoom.BuyerId == buyerId &&
                    chatRoom.SellerId == sellerId,
                    cancellationToken);
        }

        public async Task<bool> ChatRoomHasParticipantAsync(Guid chatRoomId, Guid participantId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatRooms
                .AsNoTracking()
                .AnyAsync(chatRoom =>
                    chatRoom.Id == chatRoomId &&
                    (chatRoom.BuyerId == participantId || chatRoom.SellerId == participantId),
                    cancellationToken);
        }

        public async Task<bool> ReportTargetExistsAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
        {
            return targetType switch
            {
                ReportTargetType.Product => await _dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(product => product.Id == targetId, cancellationToken),
                ReportTargetType.Seller => await _dbContext.SellerProfiles
                    .AsNoTracking()
                    .AnyAsync(profile => profile.Id == targetId, cancellationToken),
                ReportTargetType.Message => await _dbContext.Messages
                    .AsNoTracking()
                    .AnyAsync(message => message.Id == targetId, cancellationToken),
                _ => false
            };
        }

        public async Task<bool> ReportIsUniqueAsync(Guid reporterId, ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.Reports
                .AsNoTracking()
                .AnyAsync(report =>
                    report.ReporterId == reporterId &&
                    report.TargetType == targetType &&
                    report.TargetId == targetId,
                    cancellationToken);
        }
    }
}
