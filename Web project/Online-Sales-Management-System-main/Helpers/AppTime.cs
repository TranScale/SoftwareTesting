using System;

namespace OnlineSalesManagementSystem.Helpers;

/// <summary>
/// Kept for backward compatibility. Please use OnlineSalesManagementSystem.Services.Common.AppTime instead.
/// </summary>
[Obsolete("Use OnlineSalesManagementSystem.Services.Common.AppTime instead.")]
public static class AppTimeHelper
{
    public static DateTime ToVietnamTime(DateTime dt) => Services.Common.AppTime.ToVietnamTime(dt);
    public static DateTime VietnamNow() => Services.Common.AppTime.VietnamNow();
    public static DateTime UtcNow() => DateTime.UtcNow;
}
