using AutoRetainer.Modules.Voyage;

namespace AutoRetainer.Modules;

internal static class NewYesAlreadyManager
{
    static bool WasChanged = false;
    static bool IsBusy => Utils.IsBusy || VoyageScheduler.Enabled;
    internal static void Tick()
    {
        if (WasChanged)
        {
            if (!IsBusy)
            {
                WasChanged = false;
                Unlock();
                PluginLog.Debug($"YesAlready unlocked");
            }
        }
        else
        {
            if (IsBusy)
            {
                WasChanged = true;
                Lock();
                PluginLog.Debug($"YesAlready locked");
            }
        }
    }
    internal static void Lock()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("YesAlready.StopRequests", out var data))
        {
            data.Add(P.Name);
        }
    }

    internal static void Unlock()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("YesAlready.StopRequests", out var data))
        {
            data.Remove(P.Name);
        }
    }

    internal static bool? WaitForYesAlreadyDisabledTask()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("YesAlready.StopRequests", out var data))
        {
            return data.Contains(P.Name);
        }
        return true;
    }
}
