using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.API.Validators
{
    public sealed class UpdateSellerProfileRequestValidator : AbstractValidator<UpdateSellerProfileRequest>
    {
        public UpdateSellerProfileRequestValidator()
        {
            RuleFor(request => request.SellerProfileId)
                .NotEmpty();

            RuleFor(request => request.DisplayName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Bio)
                .MaximumLength(1000);

            RuleFor(request => request.Status)
                .IsInEnum();
        }
    }
}
