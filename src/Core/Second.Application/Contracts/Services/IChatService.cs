using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;

namespace Second.Application.Contracts.Services
{
    public interface IChatService
    {
        Task<ChatRoomDto> StartChatAsync(StartChatRequest request, CancellationToken cancellationToken = default);

        Task<ChatRoomDto?> GetChatRoomAsync(Guid chatRoomId, CancellationToken cancellationToken = default);

        Task<PagedResult<ChatRoomDto>> GetChatRoomsForUserAsync(Guid userId, PageRequest pageRequest, CancellationToken cancellationToken = default);

        Task<MessageDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default);

        Task<PagedResult<MessageDto>> GetMessagesAsync(Guid chatRoomId, PageRequest pageRequest, CancellationToken cancellationToken = default);
    }
}
