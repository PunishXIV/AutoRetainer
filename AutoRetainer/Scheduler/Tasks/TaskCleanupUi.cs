using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.Scheduler.Tasks;

public static class TaskCleanupUi
{
    public static void Enqueue()
    {
        EzThrottler.Throttle("RCUIAddonDetected", 10000, true);
        P.TaskManager.Enqueue(RecursivelyCleanupUI, new(timeLimitMS: 5 * 60 * 1000));
    }

    public static string[] StandardAddons => field ??= [..BailoutManager.MonitoredAddons.Select(x => x.Name)];

    public static unsafe bool RecursivelyCleanupUI()
    {
        foreach(var x in StandardAddons)
        {
            if(TryGetAddonByName<AtkUnitBase>(x, out var addon))
            {
                EzThrottler.Throttle("RCUIAddonDetected", 10000, true);
                if(addon->IsReady && EzThrottler.Throttle($"CloseAddon{x}"))
                {
                    Callback.Fire(addon, true, -1);
                }
            }
        }
        return EzThrottler.Check("RCUIAddonDetected");
    }
}
