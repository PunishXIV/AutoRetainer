using System.Diagnostics;

namespace AutoRetainer.Modules;

internal static class PriorityManager
{
    internal static bool PriorityChanged = false;

    internal static void HighPriority()
    {
        if(Process.GetCurrentProcess().PriorityClass != ProcessPriorityClass.High)
        {
            PriorityChanged = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            PluginLog.Debug($"Setting FFXIV process priority to high.");
        }
    }

    internal static void RestorePriority()
    {
        if(PriorityChanged)
        {
            PriorityChanged = false;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            PluginLog.Debug($"Restoring FFXIV process priority.");
        }
    }

    internal static void Tick()
    {
        if(C.ManipulatePriority)
        {
            if(Utils.IsBusy)
            {
                HighPriority();
            }
            else
            {
                RestorePriority();
            }
        }
    }
}
