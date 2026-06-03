using System;
using System.Collections.Generic;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}

public class Doctor : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Specialization { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Qualifications { get; set; }
    public int YearsOfExperience { get; set; } = 0;
    public decimal Rating { get; set; } = 0.0m;
    public int TotalReviews { get; set; } = 0;
    public bool IsVerified { get; set; } = false;
    
    // Onboarding and verification fields
    public string? ApplicationStatus { get; set; } // "Pending", "Approved", "Rejected"
    public DateTime? ApplicationSubmittedAt { get; set; }
    public DateTime? ApplicationReviewedAt { get; set; }
    public string? Bio { get; set; }
    public string? IdentificationDocument { get; set; } // URL or file path
    public string? CertificateDocument { get; set; } // URL or file path
    
    public virtual User User { get; set; } = null!;
    public virtual Department? Department { get; set; }
    public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class Admin : BaseEntity
{
    public Guid UserId { get; set; }
    
    public virtual User User { get; set; } = null!;
}
