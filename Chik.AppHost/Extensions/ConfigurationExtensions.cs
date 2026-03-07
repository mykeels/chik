using Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static string DatabaseUsername = "postgres";
    public static string DatabasePort = "5432";

    public static string GetDatabaseHost(this IConfiguration configuration)
    {
        return configuration["Parameters:pg-host"] ?? throw new Exception("pg-host is not set");
    }

    public static string GetDatabaseUrl(this IConfiguration configuration)
    {
        return configuration["Parameters:fusionauthdb-url"] ?? throw new Exception("fusionauthdb-url is not set");
    }

    public static string GetDatabasePassword(this IConfiguration configuration)
    {
        return configuration["Parameters:pg-password"] ?? throw new KeyNotFoundException("pg-password is not set");
    }

    public static string GetLocalDatabaseBackupDir(this IConfiguration configuration)
    {
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return configuration["Parameters:pg-backup-dir"] ?? Path.Combine(homeDirectory, "pgbackups");
    }

    public static string GetPgBackWebEncryptionKey(this IConfiguration configuration)
    {
        return configuration["Parameters:pg-back-web-encryption-key"] ?? throw new KeyNotFoundException("pg-back-web-encryption-key is not set");
    }
}