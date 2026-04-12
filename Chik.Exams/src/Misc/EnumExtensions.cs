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

    public static int ToInt32<TEnum>(this List<TEnum> roles) where TEnum : struct, Enum
    {
        // use bitmask to convert the roles to an integer
        int result = 0;
        foreach (var role in roles)
        {
            result |= Convert.ToInt32(role);
        }
        return result;
    }

    public static List<TEnum> FromInt32<TEnum>(int value) where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Where(role => (value & Convert.ToInt32(role)) == Convert.ToInt32(role)).ToList();
    }

    public static List<TEnum> ToEnumList<TEnum>(this int value) where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Where(role => (value & Convert.ToInt32(role)) == Convert.ToInt32(role)).ToList();
    }
}