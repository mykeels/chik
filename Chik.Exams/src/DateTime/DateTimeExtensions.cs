namespace Chik.Exams;

public static class DateTimeExtensions
{
    internal static DateTime Today(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
    }
    
    public static int CalculateAge(this DateOnly dateOfBirth)
    {
        var timeProvider = Provider.GetRequiredService<TimeProvider>();
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var age = today.Year - dateOfBirth.Year;
        var monthDiff = today.Month - dateOfBirth.Month;
        if (monthDiff < 0 || (monthDiff == 0 && today.Day < dateOfBirth.Day)) {
            age--;
        }
        return Math.Max(0, age);
    }
    
    public static (int Years, int Months) CalculateYearsAndMonths(this DateOnly dateOfBirth)
    {
        var timeProvider = Provider.GetRequiredService<TimeProvider>();
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var age = today.Year - dateOfBirth.Year;
        var monthDiff = today.Month - dateOfBirth.Month;
        if (monthDiff < 0 || (monthDiff == 0 && today.Day < dateOfBirth.Day)) {
            age--;
        }
        int years = Math.Max(0, age);
        int months = Math.Max(0, monthDiff);
        return (years, months);
    }
}