using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Second.API.Models;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;
using Second.Domain.Enums;

namespace Second.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var validationResult = ValidatePagination(pagination);
            if (validationResult is not null)
            {
                return validationResult;
            }

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
            var validationResult = ValidatePagination(pagination);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var pageRequest = new PageRequest { PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
            var reports = await _reportService.GetByReporterAsync(reporterId, pageRequest, cancellationToken);
            return Ok(reports);
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
