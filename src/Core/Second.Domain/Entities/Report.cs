using System;
using Second.Domain.Entities.Common;
using Second.Domain.Enums;

namespace Second.Domain.Entities
{
    public class Report : BaseEntity
    {
        public Guid ReporterId { get; set; }

        public ReportTargetType TargetType { get; set; }

        public Guid TargetId { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
