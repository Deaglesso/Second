using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Persistence;
using Second.Application.Dtos.Requests;
using Second.Domain.Enums;

namespace Second.Application.Validators
{
    public sealed class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
    {
        private readonly IAppDbContext _dbContext;

        public CreateReportRequestValidator(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

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
            return request.TargetType switch
            {
                ReportTargetType.Product => await _dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(product => product.Id == request.TargetId, cancellationToken),
                ReportTargetType.Seller => await _dbContext.SellerProfiles
                    .AsNoTracking()
                    .AnyAsync(profile => profile.Id == request.TargetId, cancellationToken),
                ReportTargetType.Message => await _dbContext.Messages
                    .AsNoTracking()
                    .AnyAsync(message => message.Id == request.TargetId, cancellationToken),
                _ => false
            };
        }

        private async Task<bool> ReportIsUniqueAsync(CreateReportRequest request, CancellationToken cancellationToken)
        {
            return !await _dbContext.Reports
                .AsNoTracking()
                .AnyAsync(report =>
                    report.ReporterId == request.ReporterId &&
                    report.TargetType == request.TargetType &&
                    report.TargetId == request.TargetId,
                    cancellationToken);
        }
    }
}
