using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Persistence;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class UpdateSellerProfileRequestValidator : AbstractValidator<UpdateSellerProfileRequest>
    {
        private readonly IAppDbContext _dbContext;

        public UpdateSellerProfileRequestValidator(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(request => request.SellerProfileId)
                .NotEmpty()
                .MustAsync(ProfileExistsAsync)
                .WithMessage("Seller profile not found. Refresh the profile and try again.");

            RuleFor(request => request.DisplayName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Bio)
                .MaximumLength(1000);

            RuleFor(request => request.Status)
                .IsInEnum();

            RuleFor(request => request)
                .MustAsync(DisplayNameUniqueAsync)
                .WithMessage("Display name is already taken. Choose a different display name.");
        }

        private async Task<bool> ProfileExistsAsync(Guid sellerProfileId, CancellationToken cancellationToken)
        {
            return await _dbContext.SellerProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.Id == sellerProfileId, cancellationToken);
        }

        private async Task<bool> DisplayNameUniqueAsync(UpdateSellerProfileRequest request, CancellationToken cancellationToken)
        {
            return !await _dbContext.SellerProfiles
                .AsNoTracking()
                .AnyAsync(profile =>
                    profile.DisplayName == request.DisplayName &&
                    profile.Id != request.SellerProfileId,
                    cancellationToken);
        }
    }
}
