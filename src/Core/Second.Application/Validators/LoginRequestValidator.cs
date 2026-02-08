using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(request => request.Password)
                .NotEmpty();
        }
    }
}
