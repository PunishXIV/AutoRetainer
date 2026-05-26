using AutoRetainer.Modules.Voyage;
using ECommons.Throttlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.Services;

public class AetherBagsManager : IDisposable
{
    string Identifier => $"{Svc.PluginInterface.Manifest.InternalName}_{ECommonsMain.InstanceUniqueId:X8}";
    volatile string Token = null;
    private bool IsBusy => VoyageScheduler.Enabled || P.TaskManager.IsBusy || AutoGCHandin.Operation;
    private AetherBagsManager()
    {
    }

    public void OnUpdate()
    {
        if(!AetherBags.Available) return;
        if(IsBusy) 
        {
            if(Token == null)
            {
                AcquireLock();
                PluginLog.Debug($"AetherBags locked");
            }
            else
            {
                if(EzThrottler.Check("ReacquireAetherBagsToken"))
                {
                    AetherBags.ReleaseVanillaInventoryBypass(Token);
                    AcquireLock();
                    PluginLog.Debug($"AetherBags lock refresh");
                }
            }
        }
        else
        {
            if(Token != null)
            {
                AetherBags.ReleaseVanillaInventoryBypass(Token);
                Token = null;
                PluginLog.Debug($"AetherBags unlocked");
            }
        }
    }

    private void AcquireLock()
    {
        Token = AetherBags.AcquireVanillaInventoryBypass(this.Identifier, 10000);
        EzThrottler.Throttle("ReacquireAetherBagsToken", 5000, true);
    }

    public void Dispose()
    {
        Svc.Framework.RunOnFrameworkThread(() =>
        {
            if(Token != null)
            {
                AetherBags.ReleaseVanillaInventoryBypass(Token);
                PluginLog.Debug($"AetherBags force unlocked due to plugin disposal");
            }
        });
    }
}
