using FluentValidation;
using Second.Application.Contracts.Services;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class StartChatRequestValidator : AbstractValidator<StartChatRequest>
    {
        private readonly IEntityValidationService _validationService;

        public StartChatRequestValidator(IEntityValidationService validationService)
        {
            _validationService = validationService;

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
            return await _validationService.ProductExistsAsync(productId, cancellationToken);
        }

        private async Task<bool> ProductIsActiveAsync(StartChatRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.ProductIsActiveAsync(request.ProductId, cancellationToken);
        }

        private async Task<bool> SellerMatchesProductAsync(StartChatRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.SellerMatchesProductAsync(request.ProductId, request.SellerId, cancellationToken);
        }

        private async Task<bool> ChatRoomDoesNotExistAsync(StartChatRequest request, CancellationToken cancellationToken)
        {
            return !await _validationService.ChatRoomExistsForParticipantsAsync(
                request.ProductId,
                request.BuyerId,
                request.SellerId,
                cancellationToken);
        }
    }
}
