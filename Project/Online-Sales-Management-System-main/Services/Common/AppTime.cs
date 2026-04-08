// FILE: OnlineSalesManagementSystem/Services/Common/AppTime.cs
using System;

namespace OnlineSalesManagementSystem.Services.Common
{
    /// <summary>
    /// Helpers for consistent time handling (store UTC, display Vietnam time).
    /// </summary>
    public static class AppTime
    {
        // Works on Linux. On Windows, fallback is used.
        private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                // Windows time zone id
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }

        /// <summary>
        /// Convert a stored UTC DateTime (or Unspecified treated as UTC) to Vietnam local time.
        /// </summary>
        public static DateTime ToVietnamTime(DateTime utcOrUnspecified)
        {
            var utc = utcOrUnspecified;

            if (utc.Kind == DateTimeKind.Local)
                utc = utc.ToUniversalTime();
            else if (utc.Kind == DateTimeKind.Unspecified)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, VietnamTimeZone);
        }

        /// <summary>
        /// Convert a Vietnam local time DateTime (or Unspecified treated as Vietnam local) to UTC.
        /// </summary>
        public static DateTime VietnamToUtc(DateTime vnLocalOrUnspecified)
        {
            var local = vnLocalOrUnspecified;

            if (local.Kind == DateTimeKind.Utc)
                return local;

            if (local.Kind == DateTimeKind.Unspecified)
            {
                // Treat as Vietnam local
                return TimeZoneInfo.ConvertTimeToUtc(local, VietnamTimeZone);
            }

            // Local (server local) -> UTC
            return local.ToUniversalTime();
        }

        public static DateTime UtcNow() => DateTime.UtcNow;
        public static DateTime VietnamNow() => ToVietnamTime(DateTime.UtcNow);
    }
}
