using AutoRetainerAPI.Configuration;
using Dalamud.Plugin.Ipc.Exceptions;
using ECommons.Throttlers;

namespace AutoRetainer.Scheduler;

internal static class ArtisanManager
{

    internal static bool WasPaused = false;

    internal static void ArtisanTick()
    {
        if(C.ArtisanIntegration)
        {
            if(IsCurrentlyOperating() && MultiMode.EnsureCharacterValidity(true))
            {
                try
                {
                    var bell = Utils.GetReachableRetainerBell(true);
                    if(AnyRetainersAvailable() && bell != null)
                    {
                        if(!WasPaused)
                        {
                            WasPaused = true;
                            Artisan.SetStopRequest(true);
                        }

                        if(!SchedulerMain.PluginEnabled || SchedulerMain.Reason != PluginEnableReason.Artisan)
                        {
                            SchedulerMain.EnablePlugin(PluginEnableReason.Artisan);
                            DebugLog($"Enabling AutoRetainer because of Artisan integration");
                        }
                    }
                }
                catch(IpcNotReadyError) { }
                catch(Exception ex)
                {
                    {
                        ex.Log();
                    }
                }
            }
            if(!AnyRetainersAvailable() && WasPaused)
            {
                if(IsOccupied())
                {
                    EzThrottler.Throttle("ArtisanCanReenableOccupied", 2500, true);
                }
                if(EzThrottler.Check("ArtisanCanReenableOccupied"))
                {
                    WasPaused = false;
                    Artisan.SetStopRequest(false);
                }
            }
        }
    }

    internal static bool AnyRetainersAvailable()
    {
        if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            return data.GetEnabledRetainers().Any(z => z.GetVentureSecondsRemaining() <= C.UnsyncCompensation);
        }
        return false;
    }

    internal static bool IsCurrentlyOperating()
    {
        try
        {
            return Artisan.IsListRunning() || Artisan.GetEnduranceStatus();
        }
        catch(IpcNotReadyError) { }
        catch(Exception ex)
        {
            ex.LogWarning();
        }
        return false;
    }
}
