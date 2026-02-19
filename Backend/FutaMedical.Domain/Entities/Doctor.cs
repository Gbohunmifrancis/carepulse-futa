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
    public Guid DepartmentId { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Qualifications { get; set; }
    public int YearsOfExperience { get; set; } = 0;
    public decimal Rating { get; set; } = 0.0m;
    public int TotalReviews { get; set; } = 0;
    public bool IsVerified { get; set; } = false;
    
    public virtual User User { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
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
