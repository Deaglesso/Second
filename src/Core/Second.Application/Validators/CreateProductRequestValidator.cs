using FluentValidation;
using Second.Application.Contracts.Services;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        private const int MaxActiveListings = 5;

        private readonly IEntityValidationService _validationService;

        public CreateProductRequestValidator(IEntityValidationService validationService)
        {
            _validationService = validationService;

            RuleFor(request => request.SellerProfileId)
                .NotEmpty()
                .MustAsync(SellerProfileExistsAsync)
                .WithMessage("Seller profile does not exist. Create a seller profile before listing products.");

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

            RuleForEach(request => request.ImageUrls)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(request => request)
                .MustAsync(TitleUniquePerSellerAsync)
                .WithMessage("A product with the same title already exists for this seller. Choose a different title.");

            RuleFor(request => request)
                .MustAsync(WithinActiveListingLimitAsync)
                .WithMessage($"Sellers can only have {MaxActiveListings} active listings. Deactivate an existing listing or upgrade your plan.");
        }

        private async Task<bool> SellerProfileExistsAsync(Guid sellerProfileId, CancellationToken cancellationToken)
        {
            return await _validationService.SellerProfileExistsAsync(sellerProfileId, cancellationToken);
        }

        private async Task<bool> TitleUniquePerSellerAsync(CreateProductRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.ProductTitleUniqueForSellerAsync(
                request.SellerProfileId,
                request.Title,
                null,
                cancellationToken);
        }

        private async Task<bool> WithinActiveListingLimitAsync(CreateProductRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.SellerHasCapacityForActiveListingAsync(
                request.SellerProfileId,
                MaxActiveListings,
                null,
                cancellationToken);
        }
    }
}
