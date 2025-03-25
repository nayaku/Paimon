using System;
using Unity.Logging;

/// <summary>
/// 计时器
/// </summary>
public class DebugStopWatcher
{
    private DateTime startTime;
    private readonly string name;

    public DebugStopWatcher(string name = "")
    {
        this.name = name;
        startTime = DateTime.Now;
        Log.Debug($"DebugStopWatcher {this.name} started at {this.startTime}");
    }

    public void Reset()
    {
        startTime = DateTime.Now;
    }

    public void Interrupt(string reason)
    {
        var currentDateTime = DateTime.Now;
        Log.Debug($"DebugStopWatcher {this.name} {reason} {(currentDateTime - startTime).TotalSeconds} seconds");
        startTime = DateTime.Now;
    }
}
