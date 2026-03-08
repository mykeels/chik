namespace Chik.Exams;

public static class ApplicationConstants
{
    private static string? _name = null;
    public static string Name
    {
        get
        {
            if (_name is not null && !string.IsNullOrEmpty(_name))
            {
                return _name;
            }
            var assembly = System.Reflection.Assembly.GetEntryAssembly()
                ?? System.Reflection.Assembly.GetExecutingAssembly();
            _name = assembly.GetName().Name ?? "Under4Games";
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public static string LogsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "services/logs");
}