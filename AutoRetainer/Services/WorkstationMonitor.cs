using Microsoft.Win32;

namespace AutoRetainer.Services;
public unsafe sealed class WorkstationMonitor : IDisposable
{
    public bool Locked { get; private set; } = false;
    private WorkstationMonitor()
    {
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if(e.Reason == SessionSwitchReason.SessionLock)
        {
            PluginLog.Debug($"Workstation locked ({DateTimeOffset.Now})");
            Locked = true;
        }
        else if(e.Reason == SessionSwitchReason.SessionUnlock)
        {
            PluginLog.Debug($"Workstation unlocked ({DateTimeOffset.Now})");
            Locked = false;
        }
    }
}