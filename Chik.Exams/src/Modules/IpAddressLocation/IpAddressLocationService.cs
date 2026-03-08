using Chik.Exams.IpAddressLocations.Repositories;

namespace Chik.Exams;

public interface IIpAddressLocationService
{
    Task<IpAddressLocation> Create(IpAddressLocation.Create ipAddressLocation);
    Task<IpAddressLocation> Update(Guid id, IpAddressLocation.Update ipAddressLocation);
    Task<IpAddressLocation> Get(Guid id);
    Task<IpAddressLocation> GetByIpAddress(string ipAddress);
    Task<IpAddressLocation> GetByCountryCode(string countryCode);
}

public class IpAddressLocationService(
    ILogger<IpAddressLocationService> logger,
    IIpAddressLocationRepository repository
) : IIpAddressLocationService
{
    public IIpAddressLocationRepository Repository => repository;

    public async Task<IpAddressLocation> Create(IpAddressLocation.Create ipAddressLocation)
    {
        var ipAddressLocationDbo = await repository.Create(ipAddressLocation);
        return (IpAddressLocation?)ipAddressLocationDbo ?? throw new InvalidOperationException("Failed to create ip address location");
    }

    public async Task<IpAddressLocation> Update(Guid id, IpAddressLocation.Update ipAddressLocation)
    {
        var ipAddressLocationDbo = await repository.Update(id, ipAddressLocation);
        return (IpAddressLocation?)ipAddressLocationDbo ?? throw new InvalidOperationException("Failed to update ip address location");
    }

    public async Task<IpAddressLocation> Get(Guid id)
    {
        var ipAddressLocationDbo = await repository.Get(id);
        return (IpAddressLocation?)ipAddressLocationDbo ?? throw new InvalidOperationException("Failed to get ip address location");
    }

    public async Task<IpAddressLocation> GetByIpAddress(string ipAddress)
    {
        var ipAddressLocationDbo = await repository.GetByIpAddress(ipAddress);
        return (IpAddressLocation?)ipAddressLocationDbo ?? throw new InvalidOperationException("Failed to get ip address location");
    }

    public async Task<IpAddressLocation> GetByCountryCode(string countryCode)
    {
        var ipAddressLocationDbo = await repository.GetByCountryCode(countryCode);
        return (IpAddressLocation?)ipAddressLocationDbo ?? throw new InvalidOperationException("Failed to get ip address location");
    }

    public async Task<List<IpAddressLocation>> Get()
    {
        var ipAddressLocationDbo = await repository.Get();
        return ipAddressLocationDbo.Select(dbo => (IpAddressLocation?)dbo ?? throw new InvalidOperationException("Failed to get ip address location")).ToList();
    }

    public async Task Delete(Guid id)
    {
        await repository.Delete(id);
    }
}

public static partial class UserExtensions
{
    /// <summary>
    /// Get the timezone for a user
    /// </summary>
    /// <param name="user">The user to get the timezone for</param>
    /// <param name="loginService">The login service to use</param>
    /// <returns>The timezone for the user</returns>
    public static async Task<TimeZoneInfo> GetTimezone(this Auth user, ILoginService? loginService = null)
    {
        loginService ??= Provider.GetRequiredService<ILoginService>();
        var lastLogin = await loginService.Repository.GetLastLogin(user.Id);
        IpAddressLocation? ipAddressLocation = lastLogin?.IpAddressLocation;
        var timezone = ipAddressLocation?.GetCountryTimezone() ?? TimeZoneInfo.Local;
        return timezone;
    }

    /// <summary>
    /// Get the local time for a user
    /// </summary>
    /// <param name="user">The user to get the local time for</param>
    /// <param name="loginService">The login service to use</param>
    /// <param name="timeProvider">The time provider to use</param>
    /// <returns>The local time for the user</returns>
    public static async Task<DateTime> GetLocalTime(this Auth user, ILoginService? loginService = null, TimeProvider? timeProvider = null)
    {
        timeProvider ??= Provider.GetRequiredService<TimeProvider>();
        var timezone = await GetTimezone(user, loginService);
        var utcNow = timeProvider.GetUtcNow();
        return TimeZoneInfo.ConvertTimeFromUtc(utcNow.DateTime, timezone);
    }
}