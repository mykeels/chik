namespace Chik.Exams;

/// <summary>
/// A record of a client error.
/// </summary>
/// <param name="Id">The unique identifier for the client error.</param>
/// <param name="UserId">The user identifier for the client error.</param>
/// <param name="ProfileId">The profile identifier for the client error.</param>
/// <param name="Error">The error message for the client error.</param>
/// <param name="DeviceFingerprint">The device fingerprint for the client error.</param>
/// <param name="CreatedAt">The date and time the client error was created.</param>
public record ServerError(
    Guid Id,
    Guid OperationId,
    string Error,
    string ErrorJson,
    long? UserId,
    string? RequestPath,
    string? RequestMethod,
    DateTime ErrorAt,
    DateTime CreatedAt
) {
    public Auth? User { get; set; }
    
    public record Create(
        Guid OperationId,
        string Error,
        string ErrorJson,
        long? UserId,
        string? RequestPath,
        string? RequestMethod,
        DateTime ErrorAt
    );

    public record Filter(
        long? UserId = null,
        Guid? OperationId = null,
        string? Text = null,
        DateTimeRange? DateRange = null,
        bool? IncludeUser = null
    );
}