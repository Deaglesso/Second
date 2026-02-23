using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required.");
        }
    }
}
