using NodaTime.TimeZones;

namespace Chik.Exams;

public record IpAddressLocation(
    Guid Id,
    string IpAddress,
    string CountryCode
) {
    public virtual List<Login>? Logins { get; set; }

    public TimeZoneInfo GetCountryTimezone()
    {
        return GetCountryTimezone(CountryCode);
    }

    public static TimeZoneInfo GetCountryTimezone(string countryCode)
    {
        var zoneLocations = TzdbDateTimeZoneSource.Default.ZoneLocations;
        var countryZoneLocations = zoneLocations?.Where(z => z.CountryCode == countryCode).ToList();
        
        if (countryZoneLocations == null || !countryZoneLocations.Any())
        {
            return TimeZoneInfo.Utc;
        }

        // Get timezone IDs for the country
        var timeZoneIds = countryZoneLocations.Select(z => z.ZoneId).ToList();
        
        // Convert to TimeZoneInfo objects
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
            .Where(t => timeZoneIds.Contains(t.Id))
            .ToList();

        if (!timeZones.Any())
        {
            return TimeZoneInfo.Utc;
        }

        // Calculate median timezone based on current UTC offset
        var timeZonesWithOffsets = timeZones
            .Select(tz => new
            {
                TimeZone = tz,
                Offset = tz.GetUtcOffset(DateTimeOffset.UtcNow)
            })
            .OrderBy(tz => tz.Offset)
            .ToList();

        // Find median timezone
        var medianIndex = timeZonesWithOffsets.Count / 2;
        var medianTimezone = timeZonesWithOffsets[medianIndex].TimeZone;

        return medianTimezone;
    }

    public record Create(string IpAddress, string CountryCode);
    public record Update(string? IpAddress, string? CountryCode);
    public record Filter(
        string? IpAddress = null,
        string? CountryCode = null
    );
}