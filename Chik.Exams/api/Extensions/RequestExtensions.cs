namespace Chik.Exams.Api;

public static class RequestExtensions
{
    /// <summary>
    /// Gets the client IP address from the request
    /// </summary>
    public static string? GetClientIpAddress(this HttpRequest request)
    {
        return request.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    public static List<string> GetClientIpAddresses(this HttpRequest req)
    {
        string? xForwardedFor = req.Headers["X-Forwarded-For"];
        var xForwardedForIpAddresses = string.IsNullOrWhiteSpace(xForwardedFor)
            ? new List<string>()
            : xForwardedFor.Split(",".ToCharArray()).Take(1).Select(x => x.Trim()).ToList();
        var ipAddresses =
            xForwardedForIpAddresses.Count > 0
                ? xForwardedForIpAddresses
                : new List<string>() { req.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty };
        return ipAddresses
            .Where(ipAddress => !string.IsNullOrWhiteSpace(ipAddress))
            .Select(ipAddress =>
            {
                string[] raOctets = ipAddress.Split(".".ToCharArray());
                string raNetwork = raOctets.Length > 1 ? raOctets[0] + "." + raOctets[1] : "";
                return raNetwork;
            })
            .ToList();
    }

    public static string? GetClientIpAddressCountry(this HttpRequest request)
    {
        return request.Headers.TryGetValue("Cf-IPCountry", out var country) ? country.ToString() : null;
    }

    /// <summary>
    /// Gets the server IP address from the request, with fallback to machine name
    /// </summary>
    public static string GetServerIpAddress(this HttpRequest request)
    {
        return request.HttpContext?.Connection?.LocalIpAddress?.ToString()
            ?? System.Environment.MachineName;
    }
}