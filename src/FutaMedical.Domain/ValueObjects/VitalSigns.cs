using System;

namespace FutaMedical.Domain.ValueObjects;

public class VitalSigns
{
    public string BloodPressure { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string Pulse { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
}
