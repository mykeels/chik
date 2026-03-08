namespace Chik.Exams.Data;

public class LoginDbo
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public Guid IpAddressLocationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual UserDbo? User { get; set; }
    public virtual IpAddressLocationDbo? IpAddressLocation { get; set; }

    public static implicit operator LoginDbo(Login login) => new()
    {
        Id = login.Id,
        UserId = login.UserId,
        IpAddressLocationId = login.IpAddressLocationId,
        CreatedAt = login.CreatedAt
    };

    public static implicit operator Login?(LoginDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.UserId,
        dbo.IpAddressLocationId,
        dbo.CreatedAt
    ) {
        User = dbo.User?.ToModel(),
        IpAddressLocation = dbo.IpAddressLocation
    };
}