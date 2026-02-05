using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;
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

        public async Task<PagedResult<ChatRoomDto>> GetChatRoomsForUserAsync(
            Guid userId,
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _chatRoomRepository.GetByUserIdAsync(
                userId,
                pageRequest.Skip,
                pageRequest.PageSize,
                cancellationToken);

            return new PagedResult<ChatRoomDto>
            {
                Items = items.Select(MapChatRoom).ToList(),
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageRequest.PageSize)
            };
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

        public async Task<PagedResult<MessageDto>> GetMessagesAsync(
            Guid chatRoomId,
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _messageRepository.GetByChatRoomIdAsync(
                chatRoomId,
                pageRequest.Skip,
                pageRequest.PageSize,
                cancellationToken);

            return new PagedResult<MessageDto>
            {
                Items = items.Select(MapMessage).ToList(),
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageRequest.PageSize)
            };
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
