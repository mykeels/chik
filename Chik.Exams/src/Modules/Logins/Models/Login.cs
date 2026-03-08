namespace Chik.Exams;

public record Login(
    Guid Id,
    long UserId,
    Guid IpAddressLocationId,
    DateTime CreatedAt
) {
    public Auth? User { get; set; }
    public IpAddressLocation? IpAddressLocation { get; set; }

    public record Create(
        long UserId,
        Guid IpAddressLocationId
    );

    public record Filter(
        long? UserId = null,
        Guid? IpAddressLocationId = null,
        DateTimeRange? DateRange = null,
        string? IpAddress = null,
        string? CountryCode = null,
        bool? IncludeUser = null,
        bool? IncludeIpAddressLocation = null
    );
}