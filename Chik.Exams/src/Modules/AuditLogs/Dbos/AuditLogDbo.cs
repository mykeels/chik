namespace Chik.Exams.Data;

public class AuditLogDbo
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Entity { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string ApplicationContext { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual UserDbo? User { get; set; }

    public static implicit operator AuditLogDbo(AuditLog auditLog) => new()
    {
        Id = auditLog.Id,
        UserId = auditLog.UserId,
        Entity = auditLog.Entity,
        EntityId = auditLog.EntityId,
        ApplicationContext = auditLog.ApplicationContext,
        OldValue = auditLog.OldValue,
        NewValue = auditLog.NewValue,
        CreatedAt = auditLog.CreatedAt
    };

    public static implicit operator AuditLog?(AuditLogDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.UserId,
        dbo.Entity,
        dbo.EntityId,
        dbo.ApplicationContext,
        dbo.OldValue,
        dbo.NewValue,
        dbo.CreatedAt
    )
    {
        User = dbo.User
    };
}
