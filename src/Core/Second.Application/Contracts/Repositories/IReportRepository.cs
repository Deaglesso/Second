using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Second.Domain.Entities;
using Second.Domain.Enums;

namespace Second.Application.Contracts.Repositories
{
    public interface IReportRepository
    {
        Task<(IReadOnlyList<Report> Items, int TotalCount)> GetByTargetAsync(
            ReportTargetType targetType,
            Guid targetId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<Report> Items, int TotalCount)> GetByReporterAsync(
            Guid reporterId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task AddAsync(Report report, CancellationToken cancellationToken = default);
    }
}
