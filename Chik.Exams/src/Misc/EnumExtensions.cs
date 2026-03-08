using System.ComponentModel;
using System.Reflection;

namespace Chik.Exams;

public static class EnumExtensions
{
    /// <summary>
    /// Get the [Description] attribute of an enum value
    /// </summary>
    /// <param name="enumValue">The enum value to get the description for.</param>
    /// <returns>The description of the enum value.</returns>
    public static string GetDescription(this Enum enumValue)
    {
        return enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
    }
}