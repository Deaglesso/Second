using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Persistence;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class AddProductImageRequestValidator : AbstractValidator<AddProductImageRequest>
    {
        private const int MaxImagesPerProduct = 10;

        private readonly IAppDbContext _dbContext;

        public AddProductImageRequestValidator(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

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
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == productId, cancellationToken);
        }

        private async Task<bool> OrderUniqueAsync(AddProductImageRequest request, CancellationToken cancellationToken)
        {
            return !await _dbContext.ProductImages
                .AsNoTracking()
                .AnyAsync(image =>
                    image.ProductId == request.ProductId &&
                    image.Order == request.Order,
                    cancellationToken);
        }

        private async Task<bool> WithinImageLimitAsync(AddProductImageRequest request, CancellationToken cancellationToken)
        {
            var currentCount = await _dbContext.ProductImages
                .AsNoTracking()
                .CountAsync(image => image.ProductId == request.ProductId, cancellationToken);

            return currentCount < MaxImagesPerProduct;
        }
    }
}
