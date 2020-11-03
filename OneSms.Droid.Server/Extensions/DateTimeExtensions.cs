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
    }
}