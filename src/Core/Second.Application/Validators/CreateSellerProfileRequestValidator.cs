using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class CreateSellerProfileRequestValidator : AbstractValidator<CreateSellerProfileRequest>
    {
        public CreateSellerProfileRequestValidator()
        {
            RuleFor(request => request.DisplayName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Bio)
                .MaximumLength(1000);
        }
    }
}
