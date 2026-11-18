using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.ExcelServices;
using ECommons.ExcelServices.Sheets;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using TerraFX.Interop.Windows;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.Cabinet;
using Cabinet = Lumina.Excel.Sheets.Cabinet;

namespace AutoRetainer.Services;

public unsafe class CabinetManager : IDisposable
{
    private CabinetManager()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreClose, ["Cabinet", "CabinetWithdraw"], OnCabinetClose);
    }

    public bool IsCabinetStateReady()
    {
        return false;
    }

    public bool ShouldExcludeItemFromProcessing(uint id, uint? condition)
    {
        if(Data.GetIMSettings(true).EnableCabinetAutoDelivery)
        {
            if(condition != null && condition < 30000) return false;
            if(this.CanStoreItemInCabinetForCurrentCharacter(id, condition)) return true; 
        }
        return false;
    }

    public uint[] CabinetValidItems => field ??= Cabinet.Values.Where(x => x.Item.RowId != 0).Select(x => x.Item.RowId).ToArray();

    private void OnCabinetClose(AddonEvent type, AddonArgs args)
    {
        OfflineDataManager.WriteOfflineData(false, true);
    }

    public bool IsCabinetReady()
    {
        return UIState.Instance()->Cabinet.State == CabinetState.Loaded
             && *(int*)((nint)(&UIState.Instance()->Cabinet) + 4) == (int)CabinetState.Loaded;
    }

    /// <summary>
    /// Returns item IDs, not cabinet IDs
    /// </summary>
    /// <param name="cached"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool TryGetStoredCabinetItems(out bool cached, out List<uint> items)
    {
        items = [];
        if(IsCabinetReady())
        {
            foreach(var x in Cabinet.Values)
            {
                if(x.Item.RowId != 0)
                {
                    if(UIState.Instance()->Cabinet.IsItemInCabinet(x.RowId))
                    {
                        items.Add(x.Item.RowId);
                    }
                }
            }
            cached = false;
            return true;
        }
        if(Data != null)
        {
            cached = true;
            items = Data.StoredCabinetItems;
            return true;
        }
        cached = default;
        return false;
    }

    public bool CanItemGoIntoCabinet(uint itemId, uint? condition)
    {
        if(condition != null && condition < 30000) return false;
        return CabinetValidItems.Contains(itemId);
    }

    public bool CanStoreItemInCabinetForCurrentCharacter(uint itemId, uint? condition)
    {
        if(condition != null && condition < 30000) return false;
        return CabinetValidItems.Contains(itemId) && !Data.StoredCabinetItems.Contains(itemId) && !Utils.IsProtected(itemId);
    }

    public IGameObject GetArmoire() => Svc.Objects.FirstOrDefault(o => o.DataId.EqualsAny<uint>(2001405, 2001406, 2001407, 2005630, 2007709));

    public void EnqueueGoToInnAndDeliverEverything()
    {
        P.TaskManager.EnqueueTask(NeoTasks.ApproachObjectViaAutomove(GetArmoire, 4.4f));
        P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(GetArmoire));
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonByName<AtkUnitBase>("Cabinet", out _)) return true;
            if(TryGetAddonMaster<AddonMaster.SelectString>(out var m) && m.IsAddonReady)
            {
                foreach(var x in m.Entries)
                {
                    if(x.Text.Contains(Svc.Data.GetExcelSheet<QuestDialogueText>(name: "custom/000/CmnDefCabinet_00082").GetRow(1).Value.GetText()))
                    {
                        if(EzThrottler.Throttle("CabSelect"))
                        {
                            x.Select();
                        }
                    }
                }
            }
            return false;
        });
        P.TaskManager.Enqueue(EnqueueAllDeliverableItems);
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonByName<AtkUnitBase>("Cabinet", out var addon))
            {
                if(addon->IsReady())
                {
                    if(EzThrottler.Throttle("CloseCabinet"))
                    {
                        Callback.Fire(addon, true, -1);
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
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
            }, new(timeLimitMS:5 * 60 * 1000)));
        }
    }

    public bool EnqueueAllDeliverableItems()
    {
        if(TryGetAddonByName<AtkUnitBase>("Cabinet", out var addon) && addon->IsReady() && TryGetStoredCabinetItems(out var cached, out var storedItems) && !cached)
        {
            foreach(var x in Utils.PlayerInventoryWithArmory)
            {
                var inv = InventoryManager.Instance()->GetInventoryContainer(x);
                for(var i = 0; i < inv->GetSize(); i++)
                {
                    var item = inv->GetInventorySlot(i);
                    if(item->GetConditionPercentage() == 100 && !Utils.IsProtected(item->ItemId))
                    {
                        var id = item->ItemId;
                        if(CanItemGoIntoCabinet(id, item->Condition) && !storedItems.Contains(id) && !Utils.IsProtected(item->ItemId))
                        {
                            P.TaskManager.Insert(() =>
                            {
                                var cabItem = Cabinet.Values.FirstOrNull(x => x.Item.RowId == id);
                                if(cabItem != null && cabItem.Value.RowId != 0 && !UIState.Instance()->Cabinet.IsItemInCabinet(cabItem.Value.RowId) && TryGetAddonByName<AtkUnitBase>("Cabinet", out var addon) && addon->IsReady())
                                {
                                    var result = UIState.Instance()->Cabinet.StoreCabinetItem(cabItem.Value.RowId);
                                    PluginLog.Debug($"Store cabinet item {ExcelItemHelper.GetName(cabItem.Value.Item.RowId, true)}, result={result}");
                                    if(EzThrottler.Check("StoreCabinet") && EzThrottler.Throttle($"StoreCabinet{cabItem}", 2000))
                                    {
                                        EzThrottler.Throttle("StoreCabinet", 333, true);
                                        P.TaskManager.InsertTask(new(() =>
                                        {
                                            var snapshot = Utils.GetCapturedInventoryState(Utils.PlayerInventoryWithArmory);
                                            P.TaskManager.InsertTask(new(() =>
                                            {
                                                return !Utils.GetCapturedInventoryState(Utils.PlayerInventoryWithArmory).SequenceEqual(snapshot);
                                            }, "Wait until inventory update", new(timeLimitMS: 5000, abortOnTimeout: false)));
                                        }, $"Store Item {ExcelItemHelper.GetName(id, true)} in cabinet")
                                        );
                                        return true;
                                    }
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }, $"Store Item {ExcelItemHelper.GetName(id, true)} in cabinet master task");
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    public bool CanDeliverCabinet()
    {
        if(TryGetStoredCabinetItems(out _, out var storedItems))
        {
            foreach(var x in Utils.PlayerInventoryWithArmory)
            {
                var inv = InventoryManager.Instance()->GetInventoryContainer(x);
                for(var i = 0; i < inv->GetSize(); i++)
                {
                    var item = inv->GetInventorySlot(i);
                    if(item->GetConditionPercentage() == 100)
                    {
                        var id = item->ItemId;
                        if(CanItemGoIntoCabinet(id, item->Condition) && !storedItems.Contains(id) && !Utils.IsProtected(id))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PreClose, ["Cabinet", "CabinetWithdraw"], OnCabinetClose);
    }
}
