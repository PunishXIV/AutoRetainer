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
            Svc.PluginInterface.GetIpcProvider<uint, object>("AutoRetainer.SetVenture").RegisterAction(SetVenture);
        }

        internal static void Shutdown()
        {
            P.DebugLog("IPC Shutdown");
            Svc.PluginInterface.GetIpcProvider<bool>("AutoRetainer.GetSuppressed").UnregisterFunc();
            Svc.PluginInterface.GetIpcProvider<bool, object>("AutoRetainer.SetSuppressed").UnregisterAction();
            Svc.PluginInterface.GetIpcProvider<uint, object>("AutoRetainer.SetVenture").UnregisterAction();
        }

        static void SetVenture(uint VentureID)
        {
            SchedulerMain.VentureOverride = VentureID;
            P.DebugLog($"Received venture override to {VentureID} / {VentureUtils.GetVentureName(VentureID)} via IPC");
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
