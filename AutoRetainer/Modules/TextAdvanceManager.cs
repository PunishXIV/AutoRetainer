﻿using AutoRetainer.Modules.Voyage;
using ECommons.EzSharedDataManager;

namespace AutoRetainer.Modules;

internal static class TextAdvanceManager
{
    private static bool WasChanged = false;

    private static bool IsBusy => (Utils.IsBusy || VoyageScheduler.Enabled) && !SchedulerMain.CharacterPostProcessLocked;
    internal static void Tick()
    {
        if(WasChanged)
        {
            if(!IsBusy)
            {
                WasChanged = false;
                UnlockTA();
                DebugLog($"TextAdvance unlocked");
            }
        }
        else
        {
            if(IsBusy)
            {
                WasChanged = true;
                LockTA();
                DebugLog($"TextAdvance locked");
            }
        }
    }
    internal static void LockTA()
    {
        if(EzSharedData.TryGet<HashSet<string>>("TextAdvance.StopRequests", out var data))
        {
            data.Add(P.Name);
        }
    }

    internal static void UnlockTA()
    {
        if(EzSharedData.TryGet<HashSet<string>>("TextAdvance.StopRequests", out var data))
        {
            data.Remove(P.Name);
        }
    }
}
