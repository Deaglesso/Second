using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Second.Application.Contracts.Repositories;
using Second.Domain.Entities;
using Second.Domain.Enums;
using Second.Persistence.Data;

namespace Second.Persistence.Implementations.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _dbContext;

        public ReportRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Report>> GetByTargetAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Reports
                .AsNoTracking()
                .Where(report => report.TargetType == targetType && report.TargetId == targetId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Report>> GetByReporterAsync(Guid reporterId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Reports
                .AsNoTracking()
                .Where(report => report.ReporterId == reporterId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
        {
            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
