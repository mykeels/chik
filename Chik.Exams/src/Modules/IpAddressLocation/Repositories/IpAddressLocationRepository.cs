using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.IpAddressLocations.Repositories;

/// <summary>
/// Every IpAddressLocation has an associated Login
/// </summary>
public interface IIpAddressLocationRepository
{
    Task<IpAddressLocationDbo> Create(IpAddressLocation.Create ipAddressLocation);
    Task<IpAddressLocationDbo?> Get(Guid id);
    Task<IpAddressLocationDbo?> GetByIpAddress(string ipAddress);
    Task<IpAddressLocationDbo?> GetByCountryCode(string countryCode);
    Task<IpAddressLocationDbo> Update(Guid id, IpAddressLocation.Update ipAddressLocation);
    Task<List<IpAddressLocationDbo>> Get(IpAddressLocation.Filter? filter = null);
    Task Delete(Guid id);
}

public class IpAddressLocationRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory, 
    ILogger<IpAddressLocationRepository> logger, 
    TimeProvider timeProvider) : IIpAddressLocationRepository
{
    public async Task<IpAddressLocationDbo> Create(IpAddressLocation.Create ipAddressLocation)
    {
        logger.LogDebug($"{nameof(IpAddressLocationRepository)}.{nameof(Create)} ({ipAddressLocation})");
        var existingIpAddressLocation = await GetByIpAddress(ipAddressLocation.IpAddress);
        if (existingIpAddressLocation is not null)
        {
            await Update(existingIpAddressLocation.Id, new IpAddressLocation.Update(ipAddressLocation.IpAddress, ipAddressLocation.CountryCode));
            return existingIpAddressLocation;
        }
        var ipAddressLocationDbo = new IpAddressLocationDbo
        {
            IpAddress = ipAddressLocation.IpAddress,
            CountryCode = ipAddressLocation.CountryCode,
        };
        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.IpAddressLocations.AddAsync(ipAddressLocationDbo);
        await dbContext.SaveChangesAsync();
        return ipAddressLocationDbo;
    }

    public async Task<IpAddressLocationDbo?> Get(Guid id)
    {
        logger.LogDebug($"{nameof(IpAddressLocationRepository)}.{nameof(Get)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.IpAddressLocations.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IpAddressLocationDbo?> GetByIpAddress(string ipAddress)
    {
        logger.LogDebug($"{nameof(IpAddressLocationRepository)}.{nameof(GetByIpAddress)} ({ipAddress})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.IpAddressLocations.FirstOrDefaultAsync(i => i.IpAddress == ipAddress);
    }

    public async Task<IpAddressLocationDbo?> GetByCountryCode(string countryCode)
    {
        logger.LogDebug($"{nameof(IpAddressLocationRepository)}.{nameof(GetByCountryCode)} ({countryCode})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.IpAddressLocations.FirstOrDefaultAsync(i => i.CountryCode == countryCode);
    }

    public async Task<IpAddressLocationDbo> Update(Guid id, IpAddressLocation.Update ipAddressLocation)
    {
        logger.LogDebug($"{nameof(IpAddressLocationRepository)}.{nameof(Update)} ({id}, {ipAddressLocation})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var existingIpAddressLocation = await dbContext.IpAddressLocations.FirstOrDefaultAsync(i => i.Id == id);
        if (existingIpAddressLocation is null)
        {
            throw new KeyNotFoundException("Ip address location not found");
        }
        if (ipAddressLocation.IpAddress is not null)
        {
            existingIpAddressLocation.IpAddress = ipAddressLocation.IpAddress;
        }
        if (ipAddressLocation.CountryCode is not null)
        {
            existingIpAddressLocation.CountryCode = ipAddressLocation.CountryCode;
        }
        await dbContext.SaveChangesAsync();
        return existingIpAddressLocation;
    }

    public async Task<List<IpAddressLocationDbo>> Get(IpAddressLocation.Filter? filter = null)
    {
        logger.LogDebug($"{nameof(IpAddressLocationRepository)}.{nameof(Get)} ({filter})");
        filter ??= new IpAddressLocation.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = dbContext.IpAddressLocations.AsNoTracking();
        if (filter.IpAddress is not null)
        {
            query = query.Where(i => i.IpAddress == filter.IpAddress);
        }
        if (filter.CountryCode is not null)
        {
            query = query.Where(i => i.CountryCode == filter.CountryCode);
        }
        return await query.ToListAsync();
    }

    public async Task Delete(Guid id)
    {
        logger.LogDebug($"{nameof(IpAddressLocationRepository)}.{nameof(Delete)} ({id})");
        var ipAddressLocation = await Get(id);
        if (ipAddressLocation is null)
        {
            return;
        }
        using var dbContext = _dbContextFactory.CreateDbContext();
        dbContext.IpAddressLocations.Remove(ipAddressLocation);
        await dbContext.SaveChangesAsync();
    }
}