using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(request => request.Token)
                .NotEmpty()
                .MaximumLength(512);
        }
    }
}
