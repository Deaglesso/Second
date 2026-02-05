using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Persistence;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class StartChatRequestValidator : AbstractValidator<StartChatRequest>
    {
        private readonly IAppDbContext _dbContext;

        public StartChatRequestValidator(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(request => request.ProductId)
                .NotEmpty()
                .MustAsync(ProductExistsAsync)
                .WithMessage("Product not found. Choose an existing product to start a chat.");

            RuleFor(request => request.BuyerId)
                .NotEmpty();

            RuleFor(request => request.SellerId)
                .NotEmpty();

            RuleFor(request => request)
                .MustAsync(ProductIsActiveAsync)
                .WithMessage("Chat can only be started for active listings. Ask the seller to reactivate the listing.");

            RuleFor(request => request)
                .MustAsync(SellerMatchesProductAsync)
                .WithMessage("Seller does not match the product owner. Verify the seller information.");

            RuleFor(request => request)
                .MustAsync(ChatRoomDoesNotExistAsync)
                .WithMessage("A chat room already exists for this product and participants. Use the existing chat.");
        }

        private async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == productId, cancellationToken);
        }

        private async Task<bool> ProductIsActiveAsync(StartChatRequest request, CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == request.ProductId && product.IsActive, cancellationToken);
        }

        private async Task<bool> SellerMatchesProductAsync(StartChatRequest request, CancellationToken cancellationToken)
        {
            var sellerUserId = await _dbContext.Products
                .AsNoTracking()
                .Where(product => product.Id == request.ProductId)
                .Select(product => product.SellerProfile != null ? product.SellerProfile.UserId : Guid.Empty)
                .FirstOrDefaultAsync(cancellationToken);

            return sellerUserId != Guid.Empty && sellerUserId == request.SellerId;
        }

        private async Task<bool> ChatRoomDoesNotExistAsync(StartChatRequest request, CancellationToken cancellationToken)
        {
            return !await _dbContext.ChatRooms
                .AsNoTracking()
                .AnyAsync(chatRoom =>
                    chatRoom.ProductId == request.ProductId &&
                    chatRoom.BuyerId == request.BuyerId &&
                    chatRoom.SellerId == request.SellerId,
                    cancellationToken);
        }
    }
}
