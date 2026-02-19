using System;
using System.Collections.Generic;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class Student : BaseEntity
{
    public Guid UserId { get; set; }
    public string MatricNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty; // 'Male', 'Female'
    public string? Address { get; set; }
    
    // Academic Information
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public int YearOfStudy { get; set; }
    
    // Health Information
    public string? BloodGroup { get; set; }
    public string? Genotype { get; set; }
    public string? Allergies { get; set; } // Comma-separated
    
    // Emergency Contact Information
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContact { get; set; } // Legacy field
    
    public bool IsVerified { get; set; } = false;
    
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<EmergencyRequest> EmergencyRequests { get; set; } = new List<EmergencyRequest>();
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
