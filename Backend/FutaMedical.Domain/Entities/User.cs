using System;
using System.Collections.Generic;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual Student? Student { get; set; }
    public virtual Doctor? Doctor { get; set; }
    public virtual Admin? Admin { get; set; }
}
