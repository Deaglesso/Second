using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Second.API.Models;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;

namespace Second.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatsController> _logger;

        public ChatsController(IChatService chatService, ILogger<ChatsController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ChatRoomDto>> StartChatAsync(
            [FromBody] StartChatRequest request,
            CancellationToken cancellationToken)
        {
            var chatRoom = await _chatService.StartChatAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetChatRoomAsync), new { chatRoomId = chatRoom.Id }, chatRoom);
        }

        [HttpGet("{chatRoomId:guid}")]
        [ActionName(nameof(GetChatRoomAsync))]
        public async Task<ActionResult<ChatRoomDto>> GetChatRoomAsync(
            Guid chatRoomId,
            CancellationToken cancellationToken)
        {
            var chatRoom = await _chatService.GetChatRoomAsync(chatRoomId, cancellationToken);
            if (chatRoom is null)
            {
                return NotFound(CreateProblemDetails("Chat room not found.", $"No chat room found with id {chatRoomId}."));
            }

            return Ok(chatRoom);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<ActionResult<PagedResult<ChatRoomDto>>> GetChatRoomsForUserAsync(
            Guid userId,
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            var validationResult = ValidatePagination(pagination);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var chatRooms = await _chatService.GetChatRoomsForUserAsync(userId, pageRequest, cancellationToken);
            return Ok(chatRooms);
        }

        [HttpPost("{chatRoomId:guid}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessageAsync(
            Guid chatRoomId,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updatedRequest = request with { ChatRoomId = chatRoomId };
                var message = await _chatService.SendMessageAsync(updatedRequest, cancellationToken);
                return Ok(message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Send message failed for chat room {ChatRoomId}.", chatRoomId);
                return NotFound(CreateProblemDetails(
                    "Chat room not found.",
                    $"No chat room found with id {chatRoomId}."));
            }
        }

        [HttpGet("{chatRoomId:guid}/messages")]
        public async Task<ActionResult<PagedResult<MessageDto>>> GetMessagesAsync(
            Guid chatRoomId,
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            var validationResult = ValidatePagination(pagination);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var messages = await _chatService.GetMessagesAsync(chatRoomId, pageRequest, cancellationToken);
            return Ok(messages);
        }

        private ActionResult? ValidatePagination(PaginationParameters pagination)
        {
            if (pagination.IsValid())
            {
                return null;
            }

            return BadRequest(CreateProblemDetails(
                "Invalid pagination parameters.",
                $"PageNumber must be >= 1 and PageSize must be between 1 and {PaginationParameters.MaxPageSize}."));
        }

        private static ProblemDetails CreateProblemDetails(string title, string detail)
        {
            return new ProblemDetails
            {
                Title = title,
                Detail = detail
            };
        }
    }
}
