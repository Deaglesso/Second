using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(request => request.ProductId)
                .NotEmpty();

            RuleFor(request => request.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(request => request.PriceText)
                .MaximumLength(100);

            RuleFor(request => request.Condition)
                .IsInEnum();
        }
    }
}
