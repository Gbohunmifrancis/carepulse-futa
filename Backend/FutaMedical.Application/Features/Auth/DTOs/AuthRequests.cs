using System;

namespace FutaMedical.Application.Features.Auth.DTOs;

public class RegisterStudentRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string MatricNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Address { get; set; }
    
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public int YearOfStudy { get; set; }
    
    public string? BloodGroup { get; set; }
    public string? Genotype { get; set; }
    public string? Allergies { get; set; }
    
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
