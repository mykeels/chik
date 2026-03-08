namespace Chik.Exams;

public record Login(
    Guid Id,
    Guid UserId,
    Guid IpAddressLocationId,
    DateTime CreatedAt
) {
    public Auth? User { get; set; }
    public IpAddressLocation? IpAddressLocation { get; set; }

    public record Create(
        Guid UserId,
        Guid IpAddressLocationId
    );

    public record Filter(
        Guid? UserId = null,
        Guid? IpAddressLocationId = null,
        DateTimeRange? DateRange = null,
        string? IpAddress = null,
        string? CountryCode = null,
        bool? IncludeUser = null,
        bool? IncludeIpAddressLocation = null
    );
}