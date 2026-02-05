using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Domain.Entities;

namespace Second.Persistence.Implementations.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IMessageRepository _messageRepository;

        public ChatService(IChatRoomRepository chatRoomRepository, IMessageRepository messageRepository)
        {
            _chatRoomRepository = chatRoomRepository;
            _messageRepository = messageRepository;
        }

        public async Task<ChatRoomDto> StartChatAsync(StartChatRequest request, CancellationToken cancellationToken = default)
        {
            var existingRoom = await _chatRoomRepository.GetByProductAndUsersAsync(
                request.ProductId,
                request.BuyerId,
                request.SellerId,
                cancellationToken);

            if (existingRoom is not null)
            {
                return MapChatRoom(existingRoom);
            }

            var chatRoom = new ChatRoom
            {
                ProductId = request.ProductId,
                BuyerId = request.BuyerId,
                SellerId = request.SellerId
            };

            await _chatRoomRepository.AddAsync(chatRoom, cancellationToken);

            return MapChatRoom(chatRoom);
        }

        public async Task<ChatRoomDto?> GetChatRoomAsync(Guid chatRoomId, CancellationToken cancellationToken = default)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId, cancellationToken);
            return chatRoom is null ? null : MapChatRoom(chatRoom);
        }

        public async Task<IReadOnlyList<ChatRoomDto>> GetChatRoomsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var chatRooms = await _chatRoomRepository.GetByUserIdAsync(userId, cancellationToken);
            return chatRooms.Select(MapChatRoom).ToList();
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(request.ChatRoomId, cancellationToken);

            if (chatRoom is null)
            {
                throw new InvalidOperationException($"Chat room {request.ChatRoomId} was not found.");
            }

            var message = new Message
            {
                ChatRoomId = request.ChatRoomId,
                SenderId = request.SenderId,
                Content = request.Content,
                SentAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message, cancellationToken);

            return MapMessage(message);
        }

        public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(Guid chatRoomId, CancellationToken cancellationToken = default)
        {
            var messages = await _messageRepository.GetByChatRoomIdAsync(chatRoomId, cancellationToken);
            return messages.Select(MapMessage).ToList();
        }

        private static ChatRoomDto MapChatRoom(ChatRoom chatRoom)
        {
            return new ChatRoomDto
            {
                Id = chatRoom.Id,
                ProductId = chatRoom.ProductId,
                BuyerId = chatRoom.BuyerId,
                SellerId = chatRoom.SellerId,
                CreatedAt = chatRoom.CreatedAt
            };
        }

        private static MessageDto MapMessage(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt,
                CreatedAt = message.CreatedAt
            };
        }
    }
}
