using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(request => request.SellerUserId)
                .NotEmpty();

            RuleFor(request => request.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(request => request.PriceText)
                .MaximumLength(100);

            RuleFor(request => request.Price)
                .GreaterThanOrEqualTo(0);

            RuleFor(request => request.Condition)
                .IsInEnum();

            RuleForEach(request => request.ImageUrls)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}
