using System;
using System.Collections.Generic;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty; // 'Admin', 'Doctor', 'Student'
    public string? Description { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
