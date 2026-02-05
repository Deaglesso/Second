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
        Task<IReadOnlyList<Report>> GetByTargetAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Report>> GetByReporterAsync(Guid reporterId, CancellationToken cancellationToken = default);

        Task AddAsync(Report report, CancellationToken cancellationToken = default);
    }
}
