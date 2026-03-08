namespace Chik.Exams;

public static class DryRun
{
    private static bool _isDryRun = false;
    public static bool IsDryRun => _isDryRun;
    public static bool IsLive => !_isDryRun;


    /// <summary>
    /// Use dry run mode.
    /// Email sending will be skipped.
    /// </summary>
    public static void UseDryRun()
    {
        _isDryRun = true;
    }

    public static void UseLive()
    {
        _isDryRun = false;
    }
}