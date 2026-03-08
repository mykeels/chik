

using System.Reflection;

namespace Chik.Exams.Api;

public static class AssemblyExtensions
{
    public static string GetCsprojVersion()
    {
        var entryAssembly = typeof(Program).Assembly;
        string version = (entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0").Split('+')[0];
        return version;
    }
}