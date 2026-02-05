using FluentValidation;
using Second.Application.Contracts.Services;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        private const int MaxActiveListings = 5;

        private readonly IEntityValidationService _validationService;

        public UpdateProductRequestValidator(IEntityValidationService validationService)
        {
            _validationService = validationService;

            RuleFor(request => request.ProductId)
                .NotEmpty()
                .MustAsync(ProductExistsAsync)
                .WithMessage("Product not found. Refresh the listing and try again.");

            RuleFor(request => request.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(request => request.PriceText)
                .MaximumLength(100);

            RuleFor(request => request.Condition)
                .IsInEnum();

            RuleFor(request => request)
                .MustAsync(TitleUniquePerSellerAsync)
                .WithMessage("Another product with this title already exists for the seller. Choose a different title.");

            RuleFor(request => request)
                .MustAsync(WithinActiveListingLimitAsync)
                .WithMessage($"Sellers can only have {MaxActiveListings} active listings. Deactivate an existing listing or upgrade your plan.");
        }

        private async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _validationService.ProductExistsAsync(productId, cancellationToken);
        }

        private async Task<bool> TitleUniquePerSellerAsync(UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var sellerProfileId = await _validationService.GetSellerProfileIdForProductAsync(request.ProductId, cancellationToken);
            if (!sellerProfileId.HasValue)
            {
                return true;
            }

            return await _validationService.ProductTitleUniqueForSellerAsync(
                sellerProfileId.Value,
                request.Title,
                request.ProductId,
                cancellationToken);
        }

        private async Task<bool> WithinActiveListingLimitAsync(UpdateProductRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsActive)
            {
                return true;
            }

            var sellerProfileId = await _validationService.GetSellerProfileIdForProductAsync(request.ProductId, cancellationToken);
            if (!sellerProfileId.HasValue)
            {
                return true;
            }

            return await _validationService.SellerHasCapacityForActiveListingAsync(
                sellerProfileId.Value,
                MaxActiveListings,
                request.ProductId,
                cancellationToken);
        }
    }
}
