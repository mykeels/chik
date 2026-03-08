using Microsoft.Extensions.DependencyInjection;
using Chik.Exams.IpAddressLocations.Repositories;
using Chik.Exams;


namespace Chik.Exams;

public static class IpAddressLocationExtensions
{
    public static IServiceCollection AddIpAddressLocation(this IServiceCollection services)
    {
        services.AddScoped<IIpAddressLocationRepository, IpAddressLocationRepository>();
        services.AddScoped<IIpAddressLocationService, IpAddressLocationService>();
        return services;
    }
}