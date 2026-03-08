using System.Text.Json.Serialization;

namespace Chik.Exams;

/// <summary>
/// A range of dates.
/// <example>
/// <code>
/// var range = DateTimeRange.FromDate(new DateTime(2021, 1, 1));
/// var range = DateTimeRange.ToDate(new DateTime(2021, 1, 1));
/// var range = DateTimeRange.Between(new DateTime(2021, 1, 1), new DateTime(2021, 1, 31));
/// var range = DateTimeRange.Past(7).Days();
/// var range = DateTimeRange.Next(3).Months();
/// var range = DateTimeRange.Today();
/// var range = DateTimeRange.ThisWeek();
/// var range = DateTimeRange.ThisMonth();
/// var range = DateTimeRange.ThisYear();
/// var range = DateTimeRange.Before(7).Years();
/// var range = DateTimeRange.After(3).Months();
/// </example>
/// </summary>
/// <param name="From">The start date of the range.</param>
/// <param name="To">The end date of the range.</param>
public abstract record DateTimeRange(DateTime? From, DateTime? To)
{
    public static DateTimeRange FromDate(DateTime from) => new FromDateTime(from);

    public static DateTimeRange ToDate(DateTime to) => new ToDateTime(to);

    public static DateTimeRange Between(DateTime from, DateTime to) =>
        new BetweenDateTimes(from, to);

    /// <summary>
    /// Creates a range for the current day (from midnight to midnight)
    /// </summary>
    public static DateTimeRange Today()
    {
        var today = DateTime.Today;
        return Between(today, today.AddDays(1).AddTicks(-1));
    }

    /// <summary>
    /// Creates a range for the current week (from Sunday to Saturday)
    /// </summary>
    public static DateTimeRange ThisWeek()
    {
        var today = DateTime.Today;
        var daysUntilSunday = (int)today.DayOfWeek;
        var start = today.AddDays(-daysUntilSunday);
        return Between(start, start.AddDays(7).AddTicks(-1));
    }

    /// <summary>
    /// Creates a range for the current month
    /// </summary>
    public static DateTimeRange ThisMonth()
    {
        var today = DateTime.Today;
        var start = new DateTime(today.Year, today.Month, 1);
        return Between(start, start.AddMonths(1).AddTicks(-1));
    }

    /// <summary>
    /// Creates a range for the current year
    /// </summary>
    public static DateTimeRange ThisYear()
    {
        var today = DateTime.Today;
        var start = new DateTime(today.Year, 1, 1);
        return Between(start, start.AddYears(1).AddTicks(-1));
    }

    /// <summary>
    /// Creates a selector for a time period before a certain date
    /// </summary>
    public static Selector Before(int amount) => new(-amount, ToDate);

    /// <summary>
    /// Creates a selector for a time period after a certain date
    /// </summary>
    public static Selector After(int amount) => new(amount, FromDate);

    /// <summary>
    /// Creates a selector for a time period before now
    /// </summary>
    public static Selector Past(int amount) => new(-amount, dateTime => Between(dateTime, Now));

    /// <summary>
    /// Creates a selector for a time period after now
    /// </summary>
    public static Selector Next(int amount) => new(amount, dateTime => Between(Now, dateTime));

    public record Dto(DateTime? From, DateTime? To) : DateTimeRange(From, To);

    public override string ToString() => $"From={From}&To={To}";

    private static DateTime Now
    {
        get { return Provider.GetService<TimeProvider>()?.GetUtcNow().DateTime ?? DateTime.UtcNow; }
    }

    /// <summary>
    /// Helper class for selecting time periods relative to now
    /// </summary>
    public class Selector
    {
        private readonly int _amount;
        private readonly Func<DateTime, DateTimeRange> _transform;

        internal Selector(int amount, Func<DateTime, DateTimeRange> transform)
        {
            _amount = amount;
            _transform = transform;
        }

        public DateTimeRange Years() => _transform(Now.AddYears(_amount));

        public DateTimeRange Months() => _transform(Now.AddMonths(_amount));

        public DateTimeRange Weeks() => _transform(Now.AddDays(_amount * 7));

        public DateTimeRange Days() => _transform(Now.AddDays(_amount));

        public DateTimeRange Hours() => _transform(Now.AddHours(_amount));

        public DateTimeRange Minutes() => _transform(Now.AddMinutes(_amount));

        public DateTimeRange Seconds() => _transform(Now.AddSeconds(_amount));
    }
}

record FromDateTime([property: JsonIgnore] DateTime from) : DateTimeRange(from, null!);

record ToDateTime([property: JsonIgnore] DateTime to) : DateTimeRange(null!, to);

record BetweenDateTimes([property: JsonIgnore] DateTime from, [property: JsonIgnore] DateTime to)
    : DateTimeRange(from, to);
