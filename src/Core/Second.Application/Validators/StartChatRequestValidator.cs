using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class StartChatRequestValidator : AbstractValidator<StartChatRequest>
    {
        public StartChatRequestValidator()
        {
            RuleFor(request => request.ProductId)
                .NotEmpty();

            RuleFor(request => request.BuyerId)
                .NotEmpty();

            RuleFor(request => request.SellerId)
                .NotEmpty();
        }
    }
}
