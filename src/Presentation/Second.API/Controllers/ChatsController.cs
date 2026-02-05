using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Second.API.Models;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;

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

            var chatRooms = await _chatService.GetChatRoomsForUserAsync(userId, cancellationToken);
            return Ok(ToPagedResult(chatRooms, pagination));
        }

        [HttpPost("{chatRoomId:guid}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessageAsync(
            Guid chatRoomId,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken)
        {
            var updatedRequest = request with { ChatRoomId = chatRoomId };
            var message = await _chatService.SendMessageAsync(updatedRequest, cancellationToken);
            return Ok(message);
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

            var messages = await _chatService.GetMessagesAsync(chatRoomId, cancellationToken);
            return Ok(ToPagedResult(messages, pagination));
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

        private static PagedResult<T> ToPagedResult<T>(IReadOnlyList<T> items, PaginationParameters pagination)
        {
            var totalCount = items.Count;
            var pagedItems = items
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToList();

            return new PagedResult<T>
            {
                Items = pagedItems,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            };
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
