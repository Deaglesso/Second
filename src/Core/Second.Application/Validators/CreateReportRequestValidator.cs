using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
    {
        public CreateReportRequestValidator()
        {
            RuleFor(request => request.TargetType)
                .IsInEnum();

            RuleFor(request => request.TargetId)
                .NotEmpty();

            RuleFor(request => request.Reason)
                .NotEmpty()
                .MaximumLength(1000);
        }
    }
}
