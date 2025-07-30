using AutoRetainerAPI.Configuration;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Services;
public sealed unsafe class AddonWatcher : IDisposable
{
    private AddonWatcher()
    {
        Svc.AddonLifecycle.RegisterListener(Dalamud.Game.Addon.Lifecycle.AddonEvent.PreSetup, "GrandCompanySupplyList", OnSupplyListSetup);
    }

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(Dalamud.Game.Addon.Lifecycle.AddonEvent.PreSetup, "GrandCompanySupplyList", OnSupplyListSetup);
    }

    private void OnSupplyListSetup(AddonEvent type, AddonArgs args)
    {
        if(Data != null && Data.GCDeliveryType != GCDeliveryType.Disabled)
        {
            var ptr = (int*)(args.Addon + 776);
            var newValue = Data.GCDeliveryType switch
            {
                GCDeliveryType.Show_All_Items => 0,
                GCDeliveryType.Hide_Gear_Set_Items => 1,
                GCDeliveryType.Hide_Armoury_Chest_Items => 2,
                _ => 2,
            };
            PluginLog.Information($"Setting exchange mode to {Data.GCDeliveryType} ({*ptr}->{newValue})");
            *ptr = newValue;
        }
    }
}