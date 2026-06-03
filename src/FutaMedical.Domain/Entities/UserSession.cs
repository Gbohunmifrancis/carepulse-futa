using System;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenJti { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
