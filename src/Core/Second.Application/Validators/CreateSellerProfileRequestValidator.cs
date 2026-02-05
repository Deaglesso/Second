using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Persistence;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class CreateSellerProfileRequestValidator : AbstractValidator<CreateSellerProfileRequest>
    {
        private readonly IAppDbContext _dbContext;

        public CreateSellerProfileRequestValidator(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(request => request.UserId)
                .NotEmpty()
                .MustAsync(UserHasNoProfileAsync)
                .WithMessage("A seller profile already exists for this user. Update the existing profile instead.");

            RuleFor(request => request.DisplayName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Bio)
                .MaximumLength(1000);

            RuleFor(request => request.DisplayName)
                .MustAsync(DisplayNameUniqueAsync)
                .WithMessage("Display name is already taken. Choose a different display name.");
        }

        private async Task<bool> UserHasNoProfileAsync(Guid userId, CancellationToken cancellationToken)
        {
            return !await _dbContext.SellerProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.UserId == userId, cancellationToken);
        }

        private async Task<bool> DisplayNameUniqueAsync(string displayName, CancellationToken cancellationToken)
        {
            return !await _dbContext.SellerProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.DisplayName == displayName, cancellationToken);
        }
    }
}
