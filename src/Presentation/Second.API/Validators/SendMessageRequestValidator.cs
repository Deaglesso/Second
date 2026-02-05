using FluentValidation;
using Second.Application.Dtos.Requests;

namespace Second.API.Validators
{
    public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
    {
        public SendMessageRequestValidator()
        {
            RuleFor(request => request.ChatRoomId)
                .NotEmpty();

            RuleFor(request => request.SenderId)
                .NotEmpty();

            RuleFor(request => request.Content)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }
}
