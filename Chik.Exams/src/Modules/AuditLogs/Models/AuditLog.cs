namespace Chik.Exams;

public record AuditLog(
    long Id,
    long UserId,
    string Service,
    long EntityId,
    string Properties,
    DateTime CreatedAt
)
{
    public User? User { get; set; }

    public record Create<T>(
        string Service,
        long EntityId,
        T Properties
    );

    public record Create(
        string Service,
        long EntityId,
        string Properties
    );

    public record Filter(
        long? UserId = null,
        string? Service = null,
        List<long>? EntityIds = null,
        DateTimeRange? DateRange = null,
        List<long>? AuditLogIds = null,
        bool? IncludeUser = null
    );
}
