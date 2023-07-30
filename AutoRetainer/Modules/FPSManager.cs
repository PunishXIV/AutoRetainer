using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal static class FPSManager
    {
        static bool WasChanged = false;
        static uint FPSInactiveValue = 0;
        static uint FPSValue = 0;
        internal static bool IsBusy => P.TaskManager.IsBusy || AutoGCHandin.Operation || AutoLogin.Instance.IsRunning;
        internal static void Tick()
        {
            if (WasChanged)
            {
                if(!IsBusy)
                {
                    WasChanged = false;
                    Svc.GameConfig.System.Set("FPSInActive", FPSInactiveValue);
                    if (C.UnlockFPSUnlimited) Svc.GameConfig.System.Set("Fps", FPSValue);
                    PluginLog.Debug($"FPS restrictions restored");
                }
            }
            else if(C.UnlockFPS)
            {
                if (IsBusy)
                {
                    WasChanged = true;
                    FPSInactiveValue = Svc.GameConfig.System.GetUInt("FPSInActive");
                    FPSValue = Svc.GameConfig.System.GetUInt("Fps");
                    Svc.GameConfig.System.Set("FPSInActive", 0);
                    if (C.UnlockFPSUnlimited) Svc.GameConfig.System.Set("Fps", 0);
                    PluginLog.Debug($"FPS restrictions removed");
                }
            }
        }

        internal static void ForceRestore()
        {
            if (WasChanged)
            {
                WasChanged = false;
                Svc.GameConfig.System.Set("FPSInActive", FPSInactiveValue);
                Svc.GameConfig.System.Set("Fps", FPSValue);
                PluginLog.Debug($"FPS restrictions restored");
            }
        }
    }
}
