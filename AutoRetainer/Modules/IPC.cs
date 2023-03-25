using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal static class IPC
    {
        internal static bool Suppressed = false;

        internal static void Init()
        {
            P.DebugLog("IPC init");
            Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetSuppressed").RegisterFunc(GetSuppressed);
            Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetSuppressed").RegisterAction(SetSuppressed);
        }

        internal static void Shutdown()
        {
            P.DebugLog("IPC Shutdown");
            Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetSuppressed").UnregisterFunc();
            Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetSuppressed").UnregisterAction();
        }

        static bool GetSuppressed()
        {
            return Suppressed;
        }

        static void SetSuppressed(bool s)
        {
            Suppressed = s;
        }
    }
}
