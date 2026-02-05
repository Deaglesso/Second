using FluentValidation;
using Second.Application.Contracts.Services;
using Second.Application.Dtos.Requests;
using Second.Domain.Enums;

namespace Second.Application.Validators
{
    public sealed class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
    {
        private readonly IEntityValidationService _validationService;

        public CreateReportRequestValidator(IEntityValidationService validationService)
        {
            _validationService = validationService;

            RuleFor(request => request.ReporterId)
                .NotEmpty();

            RuleFor(request => request.TargetType)
                .IsInEnum();

            RuleFor(request => request.TargetId)
                .NotEmpty();

            RuleFor(request => request.Reason)
                .NotEmpty()
                .MaximumLength(1000);

            RuleFor(request => request)
                .MustAsync(TargetExistsAsync)
                .WithMessage("The report target does not exist. Verify the item and try again.");

            RuleFor(request => request)
                .MustAsync(ReportIsUniqueAsync)
                .WithMessage("You have already reported this item. Wait for moderation or add details to the existing report.");
        }

        private async Task<bool> TargetExistsAsync(CreateReportRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.ReportTargetExistsAsync(
                request.TargetType,
                request.TargetId,
                cancellationToken);
        }

        private async Task<bool> ReportIsUniqueAsync(CreateReportRequest request, CancellationToken cancellationToken)
        {
            return await _validationService.ReportIsUniqueAsync(
                request.ReporterId,
                request.TargetType,
                request.TargetId,
                cancellationToken);
        }
    }
}
