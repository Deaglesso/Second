using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.API.Validators
{
    public sealed class AddProductImageRequestValidator : AbstractValidator<AddProductImageRequest>
    {
        public AddProductImageRequestValidator()
        {
            RuleFor(request => request.ProductId)
                .NotEmpty();

            RuleFor(request => request.ImageUrl)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(request => request.Order)
                .GreaterThanOrEqualTo(0);
        }
    }
}
