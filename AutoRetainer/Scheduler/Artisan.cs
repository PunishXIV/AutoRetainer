using AutoRetainerAPI.Configuration;
using ECommons.Throttlers;

namespace AutoRetainer.Scheduler
{
    internal static class Artisan
    {
        internal static bool IsListRunning => Svc.PluginInterface.GetIpcSubscriber<bool>("Artisan.IsListRunning").InvokeFunc();
        internal static bool IsListPaused => Svc.PluginInterface.GetIpcSubscriber<bool>("Artisan.IsListPaused").InvokeFunc();
        internal static bool GetStopRequest => Svc.PluginInterface.GetIpcSubscriber<bool>("Artisan.GetStopRequest").InvokeFunc();
        internal static bool GetEnduranceStatus => Svc.PluginInterface.GetIpcSubscriber<bool>("Artisan.GetEnduranceStatus").InvokeFunc();
        internal static void SetEnduranceStatus(bool b) => Svc.PluginInterface.GetIpcSubscriber<bool, object>("Artisan.IsListRunning").InvokeAction(b);
        internal static void SetListPause(bool b) => Svc.PluginInterface.GetIpcSubscriber<bool, object>("Artisan.SetListPause").InvokeAction(b);
        internal static void SetStopRequest(bool b) => Svc.PluginInterface.GetIpcSubscriber<bool, object>("Artisan.SetStopRequest").InvokeAction(b);

        internal static bool WasPaused = false;

        internal static void ArtisanTick()
        {
            if (C.ArtisanIntegration)
            {
                if (IsCurrentlyOperating() && MultiMode.EnsureCharacterValidity(true))
                {
                    if(!SchedulerMain.PluginEnabled || SchedulerMain.Reason != PluginEnableReason.Artisan)
                    {
                        SchedulerMain.EnablePlugin(PluginEnableReason.Artisan);
                        DebugLog($"Enabling AutoRetainer because of Artisan integration");
                    }
                    try
                    {
                        if (AnyRetainersAvailable())
                        {
                            if (!WasPaused)
                            {
                                WasPaused = true;
                                SetStopRequest(true);
                            }
                        }
                    }
                    catch(Exception ex) 
                    {
                        {
                            ex.Log();
                        } 
                    }
                }
                if (!AnyRetainersAvailable() && WasPaused)
                {
                    if (IsOccupied())
                    {
                        EzThrottler.Throttle("ArtisanCanReenableOccupied", 2500, true);
                    }
                    if (EzThrottler.Check("ArtisanCanReenableOccupied"))
                    {
                        WasPaused = false;
                        SetStopRequest(false);
                    }
                }
            }
        }

        internal static bool AnyRetainersAvailable()
        {
            if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
            {
                return data.GetEnabledRetainers().Any(z => z.GetVentureSecondsRemaining() <= C.UnsyncCompensation);
            }
            return false;
        }

        internal static bool IsCurrentlyOperating()
        {
            try
            {
                return IsListRunning || GetEnduranceStatus;
            }
            catch(Exception ex)
            {
                ex.LogWarning();
            }
            return false;
        }
    }
}
