using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Second.API.Models;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
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

            var reports = await _reportService.GetByTargetAsync(targetType, targetId, cancellationToken);
            return Ok(ToPagedResult(reports, pagination));
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

            var reports = await _reportService.GetByReporterAsync(reporterId, cancellationToken);
            return Ok(ToPagedResult(reports, pagination));
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

        private static PagedResult<ReportDto> ToPagedResult(IReadOnlyList<ReportDto> reports, PaginationParameters pagination)
        {
            var totalCount = reports.Count;
            var pagedItems = reports
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToList();

            return new PagedResult<ReportDto>
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
