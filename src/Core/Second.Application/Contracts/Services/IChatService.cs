using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;

namespace Second.Application.Contracts.Services
{
    public interface IChatService
    {
        Task<ChatRoomDto> StartChatAsync(StartChatRequest request, CancellationToken cancellationToken = default);

        Task<ChatRoomDto?> GetChatRoomAsync(Guid chatRoomId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ChatRoomDto>> GetChatRoomsForUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<MessageDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MessageDto>> GetMessagesAsync(Guid chatRoomId, CancellationToken cancellationToken = default);
    }
}
