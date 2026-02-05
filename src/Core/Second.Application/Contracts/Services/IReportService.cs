using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Domain.Enums;

namespace Second.Application.Contracts.Services
{
    public interface IReportService
    {
        Task<ReportDto> CreateAsync(CreateReportRequest request, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReportDto>> GetByTargetAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReportDto>> GetByReporterAsync(Guid reporterId, CancellationToken cancellationToken = default);
    }
}
