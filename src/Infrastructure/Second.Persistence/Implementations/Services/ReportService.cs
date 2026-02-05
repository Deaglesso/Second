using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
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

        public async Task<IReadOnlyList<ReportDto>> GetByTargetAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
        {
            var reports = await _reportRepository.GetByTargetAsync(targetType, targetId, cancellationToken);
            return reports.Select(MapReport).ToList();
        }

        public async Task<IReadOnlyList<ReportDto>> GetByReporterAsync(Guid reporterId, CancellationToken cancellationToken = default)
        {
            var reports = await _reportRepository.GetByReporterAsync(reporterId, cancellationToken);
            return reports.Select(MapReport).ToList();
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
