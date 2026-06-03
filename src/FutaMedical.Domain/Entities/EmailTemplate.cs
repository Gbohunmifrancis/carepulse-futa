using System;
using FutaMedical.Domain.Common;

namespace FutaMedical.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
}
