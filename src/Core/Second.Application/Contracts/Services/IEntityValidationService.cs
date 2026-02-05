using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Enums;

namespace Second.Application.Contracts.Services
{
    public interface IEntityValidationService
    {
        Task<bool> SellerProfileExistsAsync(Guid sellerProfileId, CancellationToken cancellationToken = default);

        Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<bool> ProductTitleUniqueForSellerAsync(Guid sellerProfileId, string title, Guid? excludedProductId, CancellationToken cancellationToken = default);

        Task<bool> SellerHasCapacityForActiveListingAsync(Guid sellerProfileId, int maxActiveListings, Guid? excludedProductId, CancellationToken cancellationToken = default);

        Task<Guid?> GetSellerProfileIdForProductAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<bool> ProductImageOrderUniqueAsync(Guid productId, int order, CancellationToken cancellationToken = default);

        Task<bool> ProductHasImageCapacityAsync(Guid productId, int maxImages, CancellationToken cancellationToken = default);

        Task<bool> SellerUserAvailableAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> SellerDisplayNameUniqueAsync(string displayName, Guid? excludedSellerProfileId, CancellationToken cancellationToken = default);

        Task<bool> ProductIsActiveAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<bool> SellerMatchesProductAsync(Guid productId, Guid sellerUserId, CancellationToken cancellationToken = default);

        Task<bool> ChatRoomExistsAsync(Guid chatRoomId, CancellationToken cancellationToken = default);

        Task<bool> ChatRoomExistsForParticipantsAsync(Guid productId, Guid buyerId, Guid sellerId, CancellationToken cancellationToken = default);

        Task<bool> ChatRoomHasParticipantAsync(Guid chatRoomId, Guid participantId, CancellationToken cancellationToken = default);

        Task<bool> ReportTargetExistsAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);

        Task<bool> ReportIsUniqueAsync(Guid reporterId, ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);
    }
}
