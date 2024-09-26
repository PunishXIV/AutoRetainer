using AutoRetainer.UiHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks;
public unsafe class TaskRecursivelyBuyFuel
{
    private static uint Amount = 0;
    public static void Enqueue()
    {
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonMaster<AddonMaster.SelectYesno>(out var m))
            {
                if(m.Text.Contains("ceruleum"))
                {
                    if(EzThrottler.Throttle("CeruleumYesNo")) m.Yes();
                }
            }
            if(TryGetAddonByName<AtkUnitBase>("FreeCompanyCreditShop", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderFreeCompanyCreditShop(a);
                if(Amount != reader.Credits)
                {
                    EzThrottler.Reset("CeruleumYesNo");
                    EzThrottler.Reset("FCBuy");
                    Amount = reader.Credits;
                }
                if(reader.Credits < 100) return true;
                if(EzThrottler.Throttle("FCBuy", 2000))
                {
                    new FreeCompanyCreditShop(a).Buy(0);
                }
            }
            else
            {
                return null;
            }
            return false;
        }, new(timeLimitMS:1000 * 60 * 10));
    }
}
