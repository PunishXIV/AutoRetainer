using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal static class TextAdvanceManager
    {
        static bool WasChanged = false;
        internal static void Tick()
        {
            if (WasChanged)
            {
                if (!Utils.IsBusy)
                {
                    WasChanged = false;
                    UnlockTA();
                    PluginLog.Debug($"TextAdvance unlocked");
                }
            }
            else if (C.UnlockFPS)
            {
                if (Utils.IsBusy)
                {
                    WasChanged = true;
                    LockTA();
                    PluginLog.Debug($"TextAdvance locked");
                }
            }
        }
        internal static void LockTA()
        {
            if (Svc.PluginInterface.TryGetData<HashSet<string>>("TextAdvance.StopRequests", out var data))
            {
                data.Add(P.Name);
            }
        }

        internal static void UnlockTA()
        {
            if (Svc.PluginInterface.TryGetData<HashSet<string>>("TextAdvance.StopRequests", out var data))
            {
                data.Remove(P.Name);
            }
        }
    }
}
