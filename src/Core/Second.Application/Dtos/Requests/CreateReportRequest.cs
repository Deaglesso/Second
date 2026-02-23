using System;
using System.Text.Json.Serialization;
using Second.Domain.Enums;

namespace Second.Application.Dtos.Requests
{
    public sealed record CreateReportRequest
    {
        [JsonIgnore]
        public Guid ReporterId { get; init; }

        public ReportTargetType TargetType { get; init; }

        public Guid TargetId { get; init; }

        public string Reason { get; init; } = string.Empty;
    }
}
