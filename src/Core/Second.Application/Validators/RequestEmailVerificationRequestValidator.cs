using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class RequestEmailVerificationRequestValidator : AbstractValidator<RequestEmailVerificationRequest>
    {
        public RequestEmailVerificationRequestValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(320);
        }
    }
}
