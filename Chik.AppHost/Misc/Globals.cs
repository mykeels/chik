using Microsoft.Extensions.Configuration;

public static class Globals
{
    public static IConfiguration Configuration { get; set; } = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();
}