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
    public class SellerProfilesController : ControllerBase
    {
        private readonly ISellerProfileService _sellerProfileService;
        private readonly ILogger<SellerProfilesController> _logger;

        public SellerProfilesController(ISellerProfileService sellerProfileService, ILogger<SellerProfilesController> logger)
        {
            _sellerProfileService = sellerProfileService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<SellerProfileDto>> CreateAsync(
            [FromBody] CreateSellerProfileRequest request,
            CancellationToken cancellationToken)
        {
            var profile = await _sellerProfileService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { sellerProfileId = profile.Id }, profile);
        }

        [HttpGet("{sellerProfileId:guid}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<ActionResult<SellerProfileDto>> GetByIdAsync(
            Guid sellerProfileId,
            CancellationToken cancellationToken)
        {
            var profile = await _sellerProfileService.GetByIdAsync(sellerProfileId, cancellationToken);
            if (profile is null)
            {
                return NotFound(CreateProblemDetails(
                    "Seller profile not found.",
                    $"No seller profile found with id {sellerProfileId}."));
            }

            return Ok(profile);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<ActionResult<SellerProfileDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var profile = await _sellerProfileService.GetByUserIdAsync(userId, cancellationToken);
            if (profile is null)
            {
                return NotFound(CreateProblemDetails(
                    "Seller profile not found.",
                    $"No seller profile found for user {userId}."));
            }

            return Ok(profile);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<SellerProfileDto>>> GetAllAsync(
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            var validationResult = ValidatePagination(pagination);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var profiles = await _sellerProfileService.GetAllAsync(pageRequest, cancellationToken);
            return Ok(profiles);
        }

        [HttpPut("{sellerProfileId:guid}")]
        public async Task<ActionResult<SellerProfileDto>> UpdateAsync(
            Guid sellerProfileId,
            [FromBody] UpdateSellerProfileRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updatedRequest = request with { SellerProfileId = sellerProfileId };
                var profile = await _sellerProfileService.UpdateAsync(updatedRequest, cancellationToken);
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Seller profile update failed for {SellerProfileId}.", sellerProfileId);
                return NotFound(CreateProblemDetails(
                    "Seller profile not found.",
                    $"No seller profile found with id {sellerProfileId}."));
            }
        }

        private static ProblemDetails CreateProblemDetails(string title, string detail)
        {
            return new ProblemDetails
            {
                Title = title,
                Detail = detail
            };
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
    }
}
