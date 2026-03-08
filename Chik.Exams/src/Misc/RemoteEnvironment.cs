using System.Web;

namespace Chik.Exams;

public class RemoteEnvironment
{
    private readonly string _environment;

    public RemoteEnvironment()
    {
        _environment = GetEnvironment();
    }

    public const string Development = "dev";
    public const string Production = "prod";

    public string Environment => _environment;

    public static string GetEnvironment()
    {
        string environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Development;
        return environment.ToLower().StartsWith(Development) || environment == string.Empty ? Development : Production;
    }

    public string GetBaseUrl()
    {
        return Environment == Production ? "https://exams.chik.ng" : "http://localhost:30003";
    }

    public string GetAppUrl()
    {
        return Environment == Production ? "https://exams.chik.ng" : "http://localhost:5173";
    }

    public string GetAuthenticatedUrl(string url)
    {
        var apiUrl = GetBaseUrl();
        if (apiUrl.Contains("http://localhost")) {
            return url;
        }
        return $"{apiUrl}/api/auth/login?returnUrl={HttpUtility.UrlEncode(url)}";
    }

    /// <summary>
    /// Gets the app host/domain from the app URL
    /// </summary>
    /// <returns></returns>
    public string GetAppHost()
    {
        return new Uri(GetAppUrl()).Host;
    }

    public string? GetCookieDomain()
    {
        return Environment == RemoteEnvironment.Production ? ".chik.ng" : null;
    }
}