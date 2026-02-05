using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Persistence;
using Second.Application.Dtos.Requests;

namespace Second.Application.Validators
{
    public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
    {
        private readonly IAppDbContext _dbContext;

        public SendMessageRequestValidator(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(request => request.ChatRoomId)
                .NotEmpty()
                .MustAsync(ChatRoomExistsAsync)
                .WithMessage("Chat room not found. Refresh the conversation and try again.");

            RuleFor(request => request.SenderId)
                .NotEmpty();

            RuleFor(request => request.Content)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(request => request)
                .MustAsync(SenderIsParticipantAsync)
                .WithMessage("Sender is not part of this chat room. Use a valid participant ID.");
        }

        private async Task<bool> ChatRoomExistsAsync(Guid chatRoomId, CancellationToken cancellationToken)
        {
            return await _dbContext.ChatRooms
                .AsNoTracking()
                .AnyAsync(chatRoom => chatRoom.Id == chatRoomId, cancellationToken);
        }

        private async Task<bool> SenderIsParticipantAsync(SendMessageRequest request, CancellationToken cancellationToken)
        {
            return await _dbContext.ChatRooms
                .AsNoTracking()
                .AnyAsync(chatRoom =>
                    chatRoom.Id == request.ChatRoomId &&
                    (chatRoom.BuyerId == request.SenderId || chatRoom.SellerId == request.SenderId),
                    cancellationToken);
        }
    }
}
