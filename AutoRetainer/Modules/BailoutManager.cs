using AutoRetainer.Modules.Voyage;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal unsafe static class BailoutManager
    {
        internal static bool SimulateStuckOnQuit = false;
        internal static bool SimulateStuckOnVoyagePanel = false;
        internal static long NoSelectString = long.MaxValue;

        internal static void Tick()
        {
            if (C.EnableBailout)
            {
                if (SchedulerMain.PluginEnabled || (MultiMode.Enabled && VoyageUtils.IsInVoyagePanel()))
                {
                    if (!Utils.IsBusy && !VoyageScheduler.Enabled && TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon))
                    {
                        if (Environment.TickCount64 - NoSelectString > C.BailoutTimeout * 1000)
                        {
                            if (Utils.GenericThrottle)
                            {
                                DuoLog.Warning($"[Bailout] Closing stuck SelectString window");
                                Callback.Fire(addon, true, -1);
                                NoSelectString = Environment.TickCount64;
                            }
                        }
                    }
                    else
                    {
                        NoSelectString = Environment.TickCount64;
                    }
                }
            }
        }
    }
}
