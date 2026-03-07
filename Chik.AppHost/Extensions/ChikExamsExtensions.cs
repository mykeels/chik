public static class ChikExamsExtensions
{
    public const string DbName = "qst";
    public const string DbUser = "qst";

    public static IResourceBuilder<MySqlServerResource> AddChikExamsDb(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ParameterResource> rootPassword,
        IResourceBuilder<ParameterResource> userPassword
        )
    {
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        // Get the path to the init SQL script
        var initScriptPath = Path.Combine(AppContext.BaseDirectory, "Data");
        
        return builder.AddMySql("chik-exams-db", rootPassword)
            .WithImageTag("8.2.0")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithEnvironment("MYSQL_DATABASE", DbName)
            .WithEnvironment("MYSQL_USER", DbUser)
            .WithEnvironment("MYSQL_PASSWORD", userPassword)
            .WithArgs("--default-authentication-plugin=mysql_native_password")
            .WithBindMount(initScriptPath, "/docker-entrypoint-initdb.d", isReadOnly: true)
            .WithBindMount(Path.Combine(homeDirectory, "chik_exams_mysql"), "/var/lib/mysql");
    }

    public static IResourceBuilder<ContainerResource> AddChikExamsApp(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<MySqlServerResource> db,
        IResourceBuilder<ParameterResource> rootPassword,
        IResourceBuilder<ParameterResource> userPassword
        )
    {
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        // Ensure the qst_files directory exists
        var qstFilesPath = Path.Combine(homeDirectory, "chik_exams_qst_files");
        Directory.CreateDirectory(qstFilesPath);
        
        // Path to custom entrypoint
        var entrypointPath = Path.Combine(AppContext.BaseDirectory, "Data", "docker-entrypoint.sh");
        
        return builder.AddContainer("chik-exams", "elquimista/qst", "3.11.01")
            .WithReference(db)
            .WaitFor(db)
            .WithEnvironment("DB_HOST", db.Resource.Name)
            .WithEnvironment("DB_ROOT_PASSWORD", rootPassword)
            .WithEnvironment("DB_USER", DbUser)
            .WithEnvironment("DB_PASSWORD", userPassword)
            .WithEnvironment("DB_NAME", DbName)
            .WithEnvironment("DB_DATABASE", DbName)
            .WithBindMount(entrypointPath, "/custom-entrypoint.sh", isReadOnly: true)
            .WithBindMount(qstFilesPath, "/var/www/qst/schools/qst_files")
            .WithEntrypoint("/bin/sh")
            .WithArgs("/custom-entrypoint.sh")
            .WithHttpEndpoint(targetPort: 80)
            .WithExternalHttpEndpoints();
    }
}