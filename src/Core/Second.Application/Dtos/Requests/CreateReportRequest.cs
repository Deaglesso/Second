using System;
using Second.Domain.Enums;

namespace Second.Application.Dtos.Requests
{
    public sealed record CreateReportRequest
    {
        public Guid ReporterId { get; init; }

        public ReportTargetType TargetType { get; init; }

        public Guid TargetId { get; init; }

        public string Reason { get; init; } = string.Empty;
    }
}
