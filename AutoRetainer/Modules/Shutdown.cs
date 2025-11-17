using AutoRetainer.Modules.Voyage;

using ECommons.Automation;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace AutoRetainer.Modules;

internal static unsafe class Shutdown
{
    internal static bool Active => ShutdownAt > 0 || ForceShutdownAt > 0;
    internal static long ShutdownAt = 0;
    internal static long ForceShutdownAt = 0;

    internal static void Tick()
    {
        if(ShutdownAt != 0)
        {
            if(Environment.TickCount64 > ShutdownAt && Player.Available)
            {
                if(C.ShutdownMakesNightMode)
                {
                    C.NightMode = true;
                    ShutdownAt = 0;
                    ForceShutdownAt = 0;
                }
                else
                {
                    if(MultiMode.Enabled)
                    {
                        MultiMode.Enabled = false;
                    }

                    if(!VoyageScheduler.Enabled && !SchedulerMain.PluginEnabled && !P.TaskManager.IsBusy && Utils.CanEnqueueShutdown())
                    {
                        ShutdownAt = 0;
                        Utils.EnqueueShutdown();
                    }

                    if(ForceShutdownAt != 0)
                    {
                        if(Environment.TickCount64 > ForceShutdownAt)
                        {
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }
    }
}
