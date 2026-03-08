namespace Chik.Exams.Data;

public class IpAddressLocationDbo
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public virtual List<LoginDbo>? Logins { get; set; }

    public static implicit operator IpAddressLocationDbo(IpAddressLocation ipAddressLocation) => new()
    {
        Id = ipAddressLocation.Id,
        IpAddress = ipAddressLocation.IpAddress,
        CountryCode = ipAddressLocation.CountryCode,
    };
    
    public static implicit operator IpAddressLocation?(IpAddressLocationDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.IpAddress,
        dbo.CountryCode
    );
}