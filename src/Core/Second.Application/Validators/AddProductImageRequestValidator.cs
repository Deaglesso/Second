using FluentValidation;
using Second.Application.Contracts.Services;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class AddProductImageRequestValidator : AbstractValidator<AddProductImageRequest>
    {
        private const int MaxImagesPerProduct = 10;

        private readonly IEntityValidationService _validationService;

        public AddProductImageRequestValidator(IEntityValidationService validationService)
        {
            _validationService = validationService;

            RuleFor(request => request.ProductId)
                .NotEmpty()
                .MustAsync(ProductExistsAsync)
                .WithMessage("Product does not exist. Provide a valid product ID.");

            RuleFor(request => request.ImageUrl)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(request => request.Order)
                .GreaterThanOrEqualTo(0);

            RuleFor(request => request)
                .MustAsync(OrderUniqueAsync)
                .WithMessage("An image with the same order already exists for this product. Choose a different order.");

            RuleFor(request => request)
                .MustAsync(WithinImageLimitAsync)
                .WithMessage($"Products can have up to {MaxImagesPerProduct} images. Remove an image before adding another.");
        }

        private async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _validationService.ProductExistsAsync(productId, cancellationToken);
        }

        private async Task<bool> OrderUniqueAsync(AddProductImageRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.ProductImageOrderUniqueAsync(
                request.ProductId,
                request.Order,
                cancellationToken);
        }

        private async Task<bool> WithinImageLimitAsync(AddProductImageRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.ProductHasImageCapacityAsync(
                request.ProductId,
                MaxImagesPerProduct,
                cancellationToken);
        }
    }
}
