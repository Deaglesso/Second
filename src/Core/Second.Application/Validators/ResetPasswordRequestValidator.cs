using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(request => request.Token)
                .NotEmpty()
                .MaximumLength(512);

            RuleFor(request => request.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.");
        }
    }
}
