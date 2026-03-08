namespace Chik.Exams.Data;

public class AuditLogDbo
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Service { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string Properties { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual UserDbo? User { get; set; }

    public static implicit operator AuditLogDbo(AuditLog auditLog) => new()
    {
        Id = auditLog.Id,
        UserId = auditLog.UserId,
        Service = auditLog.Service,
        EntityId = auditLog.EntityId,
        Properties = auditLog.Properties,
        CreatedAt = auditLog.CreatedAt
    };

    public static implicit operator AuditLog?(AuditLogDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.UserId,
        dbo.Service,
        dbo.EntityId,
        dbo.Properties,
        dbo.CreatedAt
    )
    {
        User = dbo.User
    };
}
