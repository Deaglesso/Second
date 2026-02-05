using System;
using Second.Domain.Enums;

namespace Second.Application.Dtos
{
    public sealed record ReportDto
    {
        public Guid Id { get; init; }

        public Guid ReporterId { get; init; }

        public ReportTargetType TargetType { get; init; }

        public Guid TargetId { get; init; }

        public string Reason { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; }
    }
}
