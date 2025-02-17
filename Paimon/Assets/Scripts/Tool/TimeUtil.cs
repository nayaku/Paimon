using System;
using Unity.Logging;

public static class TimeUtil
{
    public static string GetTimestampWithMilliseconds()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
    public static void LogDebugTimestamp(string text)
    {
        Log.Debug($"{GetTimestampWithMilliseconds()}:{text}");
    }
}
