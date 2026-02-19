using System;
using System.Collections.Generic;
using FutaMedical.Domain.Common;
using FutaMedical.Domain.ValueObjects;

namespace FutaMedical.Domain.Entities;

public class MedicalRecord : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? AppointmentId { get; set; }
    
    // Medical Information
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Vital Signs (JSONB)
    public VitalSigns? VitalSigns { get; set; }
    
    public virtual Student Student { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Appointment? Appointment { get; set; }
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

public class Prescription : BaseEntity
{
    public Guid MedicalRecordId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    
    public virtual MedicalRecord MedicalRecord { get; set; } = null!;
}
