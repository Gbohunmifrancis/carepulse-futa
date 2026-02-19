using System;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class EmergencyRequest : BaseEntity
{
    public Guid StudentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // 'Pending', 'InProgress', 'Resolved', 'Cancelled'
    public string Priority { get; set; } = "Low"; // 'Low', 'Medium', 'High'
    public string? ResponseNotes { get; set; }
    public Guid? RespondedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    public virtual Student Student { get; set; } = null!;
}

public class Review : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public string? Response { get; set; }
    public DateTime? RespondedAt { get; set; }
    
    public virtual Student Student { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Appointment Appointment { get; set; } = null!;
}

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // 'Appointment', 'Emergency', 'System'
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Changes { get; set; } // Store JSONB string or use dynamic
    public string? IpAddress { get; set; }
}

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UpdatedBy { get; set; }
}
