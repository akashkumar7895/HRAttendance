namespace HRAttendanceMVC.Domain.Helpers;

public static class TimeHelper
{
    private static readonly TimeZoneInfo? _istZone = GetIstTimeZone();

    private static TimeZoneInfo? GetIstTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
            catch
            {
                return null;
            }
        }
    }

    public static DateTime Now
    {
        get
        {
            if (_istZone != null)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _istZone);
            }
            return DateTime.UtcNow.AddHours(5).AddMinutes(30);
        }
    }

    public static DateTime Today => Now.Date;

    public static string Format12(TimeSpan? time)
    {
        if (!time.HasValue) return "--:--";
        return Today.Add(time.Value).ToString("hh:mm tt");
    }

    public static string Format24(TimeSpan? time)
    {
        if (!time.HasValue) return "";
        return time.Value.ToString(@"hh\:mm");
    }

    public static string FormatRaw(TimeSpan? time)
    {
        if (!time.HasValue) return "";
        return time.Value.ToString(@"hh\:mm\:ss");
    }
}
