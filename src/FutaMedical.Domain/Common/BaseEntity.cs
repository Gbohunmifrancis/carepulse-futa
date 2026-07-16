using System;
using System.Collections.Generic;

namespace FutaMedical.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(object domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
