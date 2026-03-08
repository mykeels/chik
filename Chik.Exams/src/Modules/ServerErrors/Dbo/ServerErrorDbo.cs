namespace Chik.Exams.Data;

public class ServerErrorDbo
{
    public Guid Id { get; set; }
    public Guid OperationId { get; set; }
    public long? UserId { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string Error { get; set; } = string.Empty;
    public string ErrorJson { get; set; } = string.Empty;
    public DateTime ErrorAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual UserDbo? User { get; set; }

    public static implicit operator ServerErrorDbo(ServerError serverError) => new()
    {
        Id = serverError.Id,
        UserId = serverError.UserId,
        RequestPath = serverError.RequestPath,
        RequestMethod = serverError.RequestMethod,
        Error = serverError.Error,
        ErrorJson = serverError.ErrorJson,
        ErrorAt = serverError.ErrorAt,
        CreatedAt = serverError.CreatedAt,
    };

    public static implicit operator ServerError?(ServerErrorDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.OperationId,
        dbo.Error,
        dbo.ErrorJson,
        dbo.UserId,
        dbo.RequestPath,
        dbo.RequestMethod,
        dbo.ErrorAt,
        dbo.CreatedAt
    ) {
        User = dbo.User,
    };
}