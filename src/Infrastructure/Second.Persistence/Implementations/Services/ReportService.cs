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
using Second.Domain.Enums;

namespace Second.Persistence.Implementations.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<ReportDto> CreateAsync(CreateReportRequest request, CancellationToken cancellationToken = default)
        {
            var report = new Report
            {
                ReporterId = request.ReporterId,
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                Reason = request.Reason
            };

            await _reportRepository.AddAsync(report, cancellationToken);

            return MapReport(report);
        }

        public async Task<PagedResult<ReportDto>> GetByTargetAsync(
            ReportTargetType targetType,
            Guid targetId,
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _reportRepository.GetByTargetAsync(
                targetType,
                targetId,
                pageRequest.Skip,
                pageRequest.PageSize,
                cancellationToken);

            return new PagedResult<ReportDto>
            {
                Items = items.Select(MapReport).ToList(),
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageRequest.PageSize)
            };
        }

        public async Task<PagedResult<ReportDto>> GetByReporterAsync(
            Guid reporterId,
            PageRequest pageRequest,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _reportRepository.GetByReporterAsync(
                reporterId,
                pageRequest.Skip,
                pageRequest.PageSize,
                cancellationToken);

            return new PagedResult<ReportDto>
            {
                Items = items.Select(MapReport).ToList(),
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageRequest.PageSize)
            };
        }

        private static ReportDto MapReport(Report report)
        {
            return new ReportDto
            {
                Id = report.Id,
                ReporterId = report.ReporterId,
                TargetType = report.TargetType,
                TargetId = report.TargetId,
                Reason = report.Reason,
                CreatedAt = report.CreatedAt
            };
        }
    }
}
