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

        public async Task<(IReadOnlyList<Report> Items, int TotalCount)> GetByTargetAsync(
            ReportTargetType targetType,
            Guid targetId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Reports
                .AsNoTracking()
                .Where(report => report.TargetType == targetType && report.TargetId == targetId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(report => report.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<Report> Items, int TotalCount)> GetByReporterAsync(
            Guid reporterId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Reports
                .AsNoTracking()
                .Where(report => report.ReporterId == reporterId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(report => report.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
        {
            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
