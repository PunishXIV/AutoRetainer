using AutoRetainer.Modules.GcHandin;
using AutoRetainer.Modules.Multi;
using ECommons.Reflection;

namespace AutoRetainer.Helpers;

internal static class YesAlready
{
    internal static bool Reenable = false;
    internal static void DisableIfNeeded()
    {
        if (DalamudReflector.TryGetDalamudPlugin("Yes Already", out var pl, false, true))
        {
            PluginLog.Information("Disabling Yes Already");
            pl.GetStaticFoP("YesAlready.Service", "Configuration").SetFoP("Enabled", false);
            Reenable = true;
        }
    }

    internal static void EnableIfNeeded()
    {
        if (Reenable && DalamudReflector.TryGetDalamudPlugin("Yes Already", out var pl, false, true))
        {
            PluginLog.Information("Enabling Yes Already");
            pl.GetStaticFoP("YesAlready.Service", "Configuration").SetFoP("Enabled", true);
            Reenable = false;
        }
    }

    internal static bool IsEnabled()
    {
        if (DalamudReflector.TryGetDalamudPlugin("Yes Already", out var pl, false, true))
        {
            return pl.GetStaticFoP("YesAlready.Service", "Configuration").GetFoP<bool>("Enabled");
        }
        return false;
    }

    internal static void Tick()
    {
        if (P.TaskManager.IsBusy || AutoLogin.Instance.IsRunning || AutoGCHandin.Operation)
        {
            if (IsEnabled())
            {
                DisableIfNeeded();
            }
        }
        else
        {
            if (Reenable)
            {
                EnableIfNeeded();
            }
        }
    }

    internal static bool? WaitForYesAlreadyDisabledTask()
    {
        return !IsEnabled();
    }
}
