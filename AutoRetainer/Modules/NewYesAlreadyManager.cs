﻿using AutoRetainer.Modules.Voyage;
using ECommons.EzSharedDataManager;

namespace AutoRetainer.Modules;

internal static class NewYesAlreadyManager
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
                Unlock();
                DebugLog($"YesAlready unlocked");
            }
        }
        else
        {
            if(IsBusy)
            {
                WasChanged = true;
                Lock();
                DebugLog($"YesAlready locked");
            }
        }
    }
    internal static void Lock()
    {
        if(EzSharedData.TryGet<HashSet<string>>("YesAlready.StopRequests", out var data))
        {
            data.Add(P.Name);
        }
    }

    internal static void Unlock()
    {
        if(EzSharedData.TryGet<HashSet<string>>("YesAlready.StopRequests", out var data))
        {
            data.Remove(P.Name);
        }
    }

    internal static bool? WaitForYesAlreadyDisabledTask()
    {
        if(EzSharedData.TryGet<HashSet<string>>("YesAlready.StopRequests", out var data))
        {
            return data.Contains(P.Name);
        }
        return true;
    }
}
