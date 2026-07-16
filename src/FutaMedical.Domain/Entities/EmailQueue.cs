using System;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class EmailQueue : BaseEntity
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateDataJson { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // 'Pending', 'Processing', 'Completed', 'Failed'
    public int Attempts { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime ScheduledFor { get; set; } = DateTime.UtcNow;
}
