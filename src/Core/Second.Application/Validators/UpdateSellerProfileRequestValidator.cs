using FluentValidation;
using Second.Application.Contracts.Services;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class UpdateSellerProfileRequestValidator : AbstractValidator<UpdateSellerProfileRequest>
    {
        private readonly IEntityValidationService _validationService;

        public UpdateSellerProfileRequestValidator(IEntityValidationService validationService)
        {
            _validationService = validationService;

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
            return await _validationService.SellerProfileExistsAsync(sellerProfileId, cancellationToken);
        }

        private async Task<bool> DisplayNameUniqueAsync(UpdateSellerProfileRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.SellerDisplayNameUniqueAsync(
                request.DisplayName,
                request.SellerProfileId,
                cancellationToken);
        }
    }
}
