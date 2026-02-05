using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Persistence;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        private const int MaxActiveListings = 5;

        private readonly IAppDbContext _dbContext;

        public UpdateProductRequestValidator(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

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
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == productId, cancellationToken);
        }

        private async Task<bool> TitleUniquePerSellerAsync(UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var sellerProfileId = await _dbContext.Products
                .AsNoTracking()
                .Where(product => product.Id == request.ProductId)
                .Select(product => product.SellerProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (sellerProfileId == Guid.Empty)
            {
                return true;
            }

            return !await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product =>
                    product.SellerProfileId == sellerProfileId &&
                    product.Id != request.ProductId &&
                    product.Title == request.Title,
                    cancellationToken);
        }

        private async Task<bool> WithinActiveListingLimitAsync(UpdateProductRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsActive)
            {
                return true;
            }

            var sellerProfileId = await _dbContext.Products
                .AsNoTracking()
                .Where(product => product.Id == request.ProductId)
                .Select(product => product.SellerProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (sellerProfileId == Guid.Empty)
            {
                return true;
            }

            var activeListings = await _dbContext.Products
                .AsNoTracking()
                .CountAsync(product =>
                    product.SellerProfileId == sellerProfileId &&
                    product.IsActive &&
                    product.Id != request.ProductId,
                    cancellationToken);

            return activeListings < MaxActiveListings;
        }
    }
}
