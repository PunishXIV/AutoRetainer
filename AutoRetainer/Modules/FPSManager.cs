using ECommons.EzSharedDataManager;

namespace AutoRetainer.Modules;

internal static class FPSManager
{
    private static bool WasChanged = false;
    private static uint FPSInactiveValue = 0;
    private static uint FPSValue = 0;
    internal static void Tick()
    {
        if(WasChanged)
        {
            if(!Utils.IsBusy)
            {
                WasChanged = false;
                Svc.GameConfig.System.Set("FPSInActive", FPSInactiveValue);
                if(C.UnlockFPSUnlimited) Svc.GameConfig.System.Set("Fps", FPSValue);
                UnlockChillFrames();
                DebugLog($"FPS restrictions restored");
            }
        }
        else if(C.UnlockFPS)
        {
            if(Utils.IsBusy)
            {
                WasChanged = true;
                FPSInactiveValue = Svc.GameConfig.System.GetUInt("FPSInActive");
                FPSValue = Svc.GameConfig.System.GetUInt("Fps");
                Svc.GameConfig.System.Set("FPSInActive", 0);
                if(C.UnlockFPSUnlimited) Svc.GameConfig.System.Set("Fps", 0);
                if(C.UnlockFPSChillFrames) LockChillFrames();
                DebugLog($"FPS restrictions removed");
            }
        }
    }

    internal static void ForceRestore()
    {
        if(WasChanged)
        {
            WasChanged = false;
            Svc.GameConfig.System.Set("FPSInActive", FPSInactiveValue);
            Svc.GameConfig.System.Set("Fps", FPSValue);
            DebugLog($"FPS restrictions restored");
        }
    }

    internal static void LockChillFrames()
    {
        if(EzSharedData.TryGet<HashSet<string>>("ChillFrames.StopRequests", out var data))
        {
            data.Add(P.Name);
        }
    }

    internal static void UnlockChillFrames()
    {
        if(EzSharedData.TryGet<HashSet<string>>("ChillFrames.StopRequests", out var data))
        {
            data.Remove(P.Name);
        }
    }
}
