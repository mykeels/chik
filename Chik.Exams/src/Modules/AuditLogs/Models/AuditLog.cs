namespace Chik.Exams;

public record AuditLog(
    long Id,
    long UserId,
    string Entity,
    long EntityId,
    string ApplicationContext,
    string OldValue,
    string NewValue,
    DateTime CreatedAt
)
{
    public User? User { get; set; }

    public record Create(
        long UserId,
        string Entity,
        long EntityId,
        string ApplicationContext,
        string OldValue,
        string NewValue
    );

    public record Filter(
        long? UserId = null,
        string? Entity = null,
        long? EntityId = null,
        DateTimeRange? DateRange = null,
        List<long>? AuditLogIds = null,
        bool? IncludeUser = null
    );
}
