using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Second.API.Models;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Exceptions;
using Second.Application.Models;
using Second.Domain.Enums;

namespace Second.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<ActionResult<ReportDto>> CreateAsync(
            [FromBody] CreateReportRequest request,
            CancellationToken cancellationToken)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (request.ReporterId != authenticatedUserId)
            {
                throw new ForbiddenAppException("You cannot file reports as another user.", "reporter_mismatch");
            }

            var report = await _reportService.CreateAsync(request, cancellationToken);
            return Ok(report);
        }

        [HttpGet("by-target")]
        public async Task<ActionResult<PagedResult<ReportDto>>> GetByTargetAsync(
            [FromQuery] ReportTargetType targetType,
            [FromQuery] Guid targetId,
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            ValidatePagination(pagination);

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var reports = await _reportService.GetByTargetAsync(targetType, targetId, pageRequest, cancellationToken);
            return Ok(reports);
        }

        [HttpGet("by-reporter/{reporterId:guid}")]
        public async Task<ActionResult<PagedResult<ReportDto>>> GetByReporterAsync(
            Guid reporterId,
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken)
        {
            ValidatePagination(pagination);

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var reports = await _reportService.GetByReporterAsync(reporterId, pageRequest, cancellationToken);
            return Ok(reports);
        }

        private static void ValidatePagination(PaginationParameters pagination)
        {
            if (pagination.IsValid())
            {
                return;
            }

            throw new BadRequestAppException(
                $"PageNumber must be >= 1 and PageSize must be between 1 and {PaginationParameters.MaxPageSize}.",
                "invalid_pagination_parameters");
        }

        private Guid GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAppException("Invalid user token.", "invalid_user_token");
            }

            return userId;
        }
    }
}
