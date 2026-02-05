using System;
using System.Threading;
using System.Threading.Tasks;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Models;
using Second.Domain.Enums;

namespace Second.Application.Contracts.Services
{
    public interface IReportService
    {
        Task<ReportDto> CreateAsync(CreateReportRequest request, CancellationToken cancellationToken = default);

        Task<PagedResult<ReportDto>> GetByTargetAsync(ReportTargetType targetType, Guid targetId, PageRequest pageRequest, CancellationToken cancellationToken = default);

        Task<PagedResult<ReportDto>> GetByReporterAsync(Guid reporterId, PageRequest pageRequest, CancellationToken cancellationToken = default);
    }
}
