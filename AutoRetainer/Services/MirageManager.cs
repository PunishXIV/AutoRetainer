using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.ExcelServices;
using ECommons.ExcelServices.Sheets;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.Services;

public unsafe class MirageManager
{
    private MirageManager()
    {
    }

    public bool HasEligibleItems()
    {
        return GlamourLog.HasAnythingToStore().HasFlag(ECommons.IPC.Subscribers.GlamourLog.StoreTargets.Dresser);
    }

    public bool ShouldExcludeItemFromProcessing(uint id, uint? condition)
    {
        if(Data.GetIMSettings(true).EnableMirageAutoDelivery)
        {
            if(condition != null && condition < 30000) return false;
            if(GlamourLog.IsItemStorable(id)) return true;
        }
        return false;
    }

    public IGameObject GetDresser() => Svc.Objects.FirstOrDefault(o => o.DataId.EqualsAny<uint>(2009439));

    public void EnqueueGoToInnAndDeliverEverything()
    {
        P.TaskManager.EnqueueTask(NeoTasks.ApproachObjectViaAutomove(GetDresser, 4.4f));
        P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(GetDresser));
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonByName<AtkUnitBase>("MiragePrismPrismBox", out var addon) && addon->IsReady()
            && TryGetAddonByName<AtkUnitBase>("MiragePrismPrismBoxCrystallize", out var addon2) && addon2->IsReady() && FFXIVClientStructs.FFXIV.Client.Game.MirageManager.Instance()->PrismBoxLoaded) return true;
            return false;
        });
        P.TaskManager.Enqueue(() =>
        {
            GlamourLog.EntrustAll();
            EzThrottler.Throttle("GlamLogBusy", 1000, true);
        });
        P.TaskManager.EnqueueDelay(1000);
        P.TaskManager.Enqueue(() =>
        {
            if(GlamourLog.IsBusy())
            {
                EzThrottler.Throttle("GlamLogBusy", 1000, true);
            }
            return EzThrottler.Check("GlamLogBusy");
        }, new(timeLimitMS:(int)TimeSpan.FromMinutes(10).TotalMilliseconds));
        P.TaskManager.Enqueue(() =>
        {
            string[] addons = ["CabinetWithdraw", "MiragePrismMirageBox", "MiragePrismPrismBoxCrystallize", "MiragePrismMiragePlate"];
            var ret = true;
            foreach(var name in addons)
            {
                if(TryGetAddonByName<AtkUnitBase>(name, out var addon))
                {
                    if(addon->IsReady())
                    {
                        if(EzThrottler.Throttle($"Close{name}addon"))
                        {
                            Callback.Fire(addon, true, -1);
                        }
                    }
                    ret = false;
                }
            }
            return ret;
        });
        if(Player.TerritoryIntendedUse != TerritoryIntendedUseEnum.Inn)
        {
            P.TaskManager.InsertMulti(new(() =>
            {
                Lifestream.EnqueueLocalInnShortcut(null);
            }),
            new(() =>
            {
                return Player.TerritoryIntendedUse == TerritoryIntendedUseEnum.Inn && !Lifestream.IsBusy() && IsScreenReady();
            }, new(timeLimitMS: 5 * 60 * 1000)));
        }
    }
}
