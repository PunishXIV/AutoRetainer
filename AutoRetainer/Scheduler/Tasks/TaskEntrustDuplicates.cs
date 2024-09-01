using AutoRetainer.Internal;
using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Handlers;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Scheduler.Tasks;

internal static unsafe class TaskEntrustDuplicates
{
    internal static bool NoDuplicates = false;
    internal static int RequestEntrustQuantity = 0;
    internal static List<(uint ID, uint Quantity)> CapturedInventoryState = [];

    internal static unsafe bool CheckNoDuplicates()
    {
        for(var rI = InventoryType.RetainerPage1; rI <= InventoryType.RetainerPage7; rI++)
        {
            var inv = FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryContainer(rI);
            if(inv == null || inv->Loaded == 0) continue;
            for(var slot = 0; slot < inv->Size; slot++)
            {
                var slotItem = inv->GetInventorySlot(slot);
                if(slotItem == null) continue;
                if(FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(slotItem->ItemId, slotItem->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality)) > 0)
                {
                    if(!Data.TransferItemsBlacklist.Contains(slotItem->ItemId))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(() => { NoDuplicates = CheckNoDuplicates(); return true; });
        P.TaskManager.Enqueue(() => { NoDuplicates = false; return true; });
        P.TaskManager.Enqueue(NewYesAlreadyManager.WaitForYesAlreadyDisabledTask);
        if(C.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(() => { if(NoDuplicates) return true; return RetainerHandlers.SelectEntrustItems(); });
        P.TaskManager.Enqueue(() => { if(NoDuplicates) return true; return RetainerHandlers.ClickEntrustDuplicates(); });
        TaskWait.Enqueue(500);
        P.TaskManager.Enqueue(UncheckBlacklistedItems);
        TaskWait.Enqueue(500);
        P.TaskManager.Enqueue(() => { if(NoDuplicates) return true; return RetainerHandlers.ClickEntrustDuplicatesConfirm(); }, 600 * 1000, false);
        TaskWait.Enqueue(500);
        P.TaskManager.Enqueue(() => { if(NoDuplicates) return true; return RetainerHandlers.ClickCloseEntrustWindow(); }, false);
        P.TaskManager.Enqueue(RetainerHandlers.CloseAgentRetainer);
    }

    internal static bool? UncheckBlacklistedItems()
    {
        if(NoDuplicates) return true;
        if(TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GenericThrottle)
            {
                var reader = new ReaderRetainerItemTransferList(addon);
                var cnt = 0;
                for(var i = 0; i < reader.Items.Count; i++)
                {
                    if(Data.TransferItemsBlacklist.Contains(reader.Items[i].ItemID))
                    {
                        cnt++;
                        PluginLog.Debug($"Removing item {reader.Items[i].ItemID} at position {i} as it was in blacklist");
                        Callback.Fire(addon, true, 0, (uint)i);
                    }
                }
                if(cnt == reader.Items.Count)
                {
                    NoDuplicates = true;
                    addon->Close(true);
                }
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    static InventoryType[] RetainerInventories = [InventoryType.RetainerPage1, InventoryType.RetainerPage2, InventoryType.RetainerPage3, InventoryType.RetainerPage4, InventoryType.RetainerPage5, InventoryType.RetainerPage6, InventoryType.RetainerPage7, InventoryType.Crystals];
    static InventoryType[] PlayerInvetories = [InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4, InventoryType.Crystals];

    public static void EnqueueNew(EntrustPlan plan)
    {
        P.TaskManager.Enqueue(RetainerHandlers.SelectEntrustItems);
        P.TaskManager.Enqueue(() => InventorySpaceManager.IsRetainerInventoryLoaded());
        P.TaskManager.Enqueue(() => RecursivelyEntrustItems(plan), timeLimitMs:60*60*1000);
        P.TaskManager.DelayNext(333);
        P.TaskManager.Enqueue(TaskVendorItems.CloseInventory);
    }

    static bool? RecursivelyEntrustItems(EntrustPlan plan)
    {
        try
        {
            if(TryGetAddonByName<AtkUnitBase>("InputNumeric", out var numeric))
            {
                if(IsAddonReady(numeric))
                {
                    var maxAmount = numeric->AtkValues[3].UInt;
                    var result = Math.Clamp(RequestEntrustQuantity, 1, maxAmount);
                    if(EzThrottler.Throttle("EntrustItemInputNumeric", 200))
                    {
                        PluginLog.Information($"Processing input numeric: {result} (max: {maxAmount})");
                        Callback.Fire(numeric, true, (int)result);
                    }
                }
                return false;
            }
            if(!EzThrottler.Check("InventoryTimeout") && Utils.GetCapturedInventoryState(PlayerInvetories).SequenceEqual(CapturedInventoryState))
            {
                return false;
            }
            if(!InventorySpaceManager.IsRetainerInventoryLoaded()) return false;
            if(EzThrottler.Throttle("EntrustItem", 333))
            {
                List<(uint ItemID, int ToKeep)> itemList = [];
                foreach(var x in plan.EntrustItems)
                {
                    itemList.Add((x, plan.EntrustItemsAmountToKeep.SafeSelect(x)));
                }
                foreach(var item in Svc.Data.GetExcelSheet<Item>())
                {
                    if(itemList.Any(s => s.ItemID == item.RowId)) continue;
                    if(plan.EntrustCategories.TryGetFirst(c => c.ID == item.ItemUICategory.Row, out var info))
                    {
                        itemList.Add((item.RowId, info.AmountToKeep));
                    }
                }
                if(plan.Duplicates)
                {
                    foreach(var type in RetainerInventories)
                    {
                        if(type == InventoryType.Crystals) continue;
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(int i = 0; i < inv->Size; i++)
                        {
                            var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                            if(item->ItemId != 0 && item->Quantity > 0)
                            {
                                if(itemList.Any(s => s.ItemID == item->ItemId)) continue;
                                var data = ExcelItemHelper.Get(item->ItemId);
                                var amountToKeep = 0;
                                if(!plan.DuplicatesMultiStack)
                                {
                                    amountToKeep = (int)(Utils.GetItemCount(PlayerInvetories, item->ItemId) - (data.StackSize - item->Quantity));
                                }
                                itemList.Add((item->ItemId, amountToKeep));
                                PluginLog.Debug($"[TED] Retainer duplicate added: {ExcelItemHelper.GetName(item->ItemId, true)}, toKeep: {amountToKeep}");
                            }
                        }
                    }
                }
                foreach(var type in PlayerInvetories)
                {
                    var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                    for(int i = 0; i < inv->Size; i++)
                    {
                        var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                        if(item->ItemId != 0 && item->Quantity > 0)
                        {
                            var itemCount = Utils.GetItemCount(PlayerInvetories, item->ItemId);
                            PluginLog.Debug($"[TED] Item count for {ExcelItemHelper.GetName(item->ItemId, true)} = {itemCount}");
                            var data = ExcelItemHelper.Get(item->ItemId);
                            if(itemList.TryGetFirst(s => s.ItemID == item->ItemId, out var entrustInfo))
                            {
                                var toKeep = entrustInfo.ToKeep;
                                var toEntrust = itemCount - toKeep;
                                PluginLog.Debug($"[TED] For {ExcelItemHelper.GetName(item->ItemId, true)} toEntrust={toEntrust}, toKeep={toKeep}");
                                if(toEntrust > 0)
                                {
                                    var toEntrustFromStack = Math.Min(item->Quantity, toEntrust);
                                    if(toEntrustFromStack > 0)
                                    {
                                        if(type == InventoryType.Crystals)
                                        {
                                            RequestEntrustQuantity = (int)toEntrustFromStack;
                                            CapturedInventoryState = Utils.GetCapturedInventoryState(PlayerInvetories);
                                            EzThrottler.Throttle("InventoryTimeout", 5000, true);
                                            P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, RetainerItemCommand.EntrustToRetainer);
                                            return false;
                                        }
                                        else
                                        {
                                            if(item->Quantity <= 1 || item->Quantity == toEntrustFromStack)
                                            {
                                                CapturedInventoryState = Utils.GetCapturedInventoryState(PlayerInvetories);
                                                EzThrottler.Throttle("InventoryTimeout", 5000, true);
                                                P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, RetainerItemCommand.EntrustToRetainer);
                                                return false;
                                            }
                                            else
                                            {
                                                //partial entrust
                                                RequestEntrustQuantity = (int)toEntrustFromStack;
                                                CapturedInventoryState = Utils.GetCapturedInventoryState(PlayerInvetories);
                                                EzThrottler.Throttle("InventoryTimeout", 5000, true);
                                                P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, RetainerItemCommand.EntrustQuantity);
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        return false;
    }
}
