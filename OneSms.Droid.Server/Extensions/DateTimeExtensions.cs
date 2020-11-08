using System;

namespace OneSms.Droid.Server.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime IgnoreMilliseconds(this DateTime value)
        {
            var newDate = new DateTime(value.Year, value.Month, value.Day,value.Hour,value.Minute,value.Second);
            return newDate;
        }

        public static DateTime FromUnixTime(this long unixTimeMillis)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTimeMillis);
        }
    }
}