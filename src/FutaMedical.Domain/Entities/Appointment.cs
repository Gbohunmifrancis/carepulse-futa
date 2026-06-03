using System;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; } // Store in UTC
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = "Pending"; // 'Pending', 'Confirmed', 'Completed', 'Cancelled', 'Rejected'
    public string ReasonForVisit { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public virtual Student Student { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual MedicalRecord? MedicalRecord { get; set; }
    public virtual Review? Review { get; set; }
}

public class DoctorAvailability : BaseEntity
{
    public Guid DoctorId { get; set; }
    public int DayOfWeek { get; set; } // 0=Sunday, 6=Saturday
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    
    public virtual Doctor Doctor { get; set; } = null!;
}
