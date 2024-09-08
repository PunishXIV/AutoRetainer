using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Handlers;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Scheduler.Tasks;

internal static unsafe class TaskEntrustDuplicates
{
    internal static int RequestEntrustQuantity = 0;
    internal static List<(uint ID, uint Quantity)> CapturedInventoryState = [];
    internal static bool WasOpen = false;

    public static void EnqueueNew(EntrustPlan plan)
    {
        P.TaskManager.Enqueue((Action)(() => WasOpen = false));
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon));
        P.TaskManager.Enqueue(() => RecursivelyEntrustItems(plan), timeLimitMs: 60 * 60 * 1000);
        P.TaskManager.Enqueue(() => !WasOpen || TaskVendorItems.CloseInventory() == true);
    }

    private static bool? RecursivelyEntrustItems(EntrustPlan plan)
    {
        try
        {
            var allowedPlayerInventories = plan.GetAllowedInventories();
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
            if(!EzThrottler.Check("InventoryTimeout") && Utils.GetCapturedInventoryState(allowedPlayerInventories).SequenceEqual(CapturedInventoryState))
            {
                return false;
            }
            if(EzThrottler.Throttle("EntrustItem", 333))
            {
                List<(uint ItemID, int ToKeep)> itemList = [];
                foreach(var x in plan.EntrustItems)
                {
                    var add = (x, plan.EntrustItemsAmountToKeep.SafeSelect(x));
                    itemList.Add(add);
                    PluginLog.Debug($"[TED] From EntrustItems added item: {ExcelItemHelper.GetName(add.Item1, true)} toKeep={add.Item2}");
                }
                foreach(var x in Utils.GetItemsInInventory(allowedPlayerInventories))
                {
                    var item = ExcelItemHelper.Get(x);
                    if(item == null) continue;
                    if(itemList.Any(s => s.ItemID == item.RowId)) continue;
                    if(plan.EntrustCategories.TryGetFirst(c => c.ID == item.ItemUICategory.Row, out var info))
                    {
                        var add = (item.RowId, info.AmountToKeep);
                        itemList.Add(add);
                        PluginLog.Debug($"[TED] From EntrustCategories added item: {ExcelItemHelper.GetName(add.Item1, true)} toKeep={add.Item2}");
                    }
                }
                if(plan.Duplicates && plan.DuplicatesMultiStack)
                {
                    foreach(var type in Utils.RetainerInventoriesWithCrystals)
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                            if(item->ItemId != 0 && item->Quantity > 0)
                            {
                                if(itemList.Any(s => s.ItemID == item->ItemId)) continue;
                                var data = ExcelItemHelper.Get(item->ItemId);
                                itemList.Add((item->ItemId, 0));
                                PluginLog.Debug($"[TED] From retainer multistack duplicate added: {ExcelItemHelper.GetName(item->ItemId, true)}");
                            }
                        }
                    }
                }
                //processing unconditional entrusts
                foreach(var type in allowedPlayerInventories)
                {
                    var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                    for(var i = 0; i < inv->Size; i++)
                    {
                        var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                        if(item->ItemId != 0 && item->Quantity > 0)
                        {
                            var itemCount = Utils.GetItemCount(allowedPlayerInventories, item->ItemId);
                            PluginLog.Debug($"[TED] Item count for {ExcelItemHelper.GetName(item->ItemId, true)} = {itemCount}");
                            var data = ExcelItemHelper.Get(item->ItemId);
                            if(itemList.TryGetFirst(s => s.ItemID == item->ItemId, out var entrustInfo))
                            {
                                var toKeep = entrustInfo.ToKeep;
                                var toEntrust = itemCount - toKeep;
                                var canFit = Utils.GetAmountThatCanFit(Utils.RetainerInventoriesWithCrystals, item->ItemId, item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality), out var debugData);
                                PluginLog.Debug($"[TED] For {ExcelItemHelper.GetName(item->ItemId, true)} toEntrust={toEntrust}, toKeep={toKeep}, canFit={canFit}\n{debugData.Print("\n")}");
                                if(toEntrust > canFit) toEntrust = (int)canFit;
                                if(toEntrust > 0)
                                {
                                    var toEntrustFromStack = Math.Min(item->Quantity, toEntrust);
                                    if(toEntrustFromStack > 0)
                                    {
                                        MoveSlotToRetainerInventoryUnsafe(item, (int)toEntrustFromStack, i, type, allowedPlayerInventories);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                if(plan.Duplicates && !plan.DuplicatesMultiStack)
                {
                    //and now processing duplicates
                    foreach(var type in Utils.RetainerInventoriesWithCrystals)
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        //find incomplete stacks, then query them from player inventory
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = inv->GetInventorySlot(i);
                            if(item->ItemId != 0 && !itemList.Any(s => s.ItemID == item->ItemId))
                            {
                                var data = ExcelItemHelper.Get(item->ItemId);
                                if(data == null || data.IsUnique) continue;
                                var canFit = data.StackSize - item->Quantity;
                                if(canFit > 0)
                                {
                                    foreach(var playerType in allowedPlayerInventories)
                                    {
                                        var playerInv = InventoryManager.Instance()->GetInventoryContainer(playerType);
                                        for(var q = 0; q < playerInv->Size; q++)
                                        {
                                            var playerItem = playerInv->GetInventorySlot(q);
                                            if(playerItem->ItemId == item->ItemId && playerItem->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality) == item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality))
                                            {
                                                var toEntrustFromStack = Math.Min(canFit, playerItem->Quantity);
                                                MoveSlotToRetainerInventoryUnsafe(playerItem, (int)toEntrustFromStack, q, playerType, allowedPlayerInventories);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="toEntrustFromStack"></param>
    /// <param name="i">slot id</param>
    /// <param name="type"></param>
    private static void MoveSlotToRetainerInventoryUnsafe(InventoryItem* item, int toEntrustFromStack, int i, InventoryType type, InventoryType[] allowedPlayerInventories)
    {
        if(!InventorySpaceManager.IsRetainerInventoryLoaded())
        {
            if(EzThrottler.Throttle("REI SelectEntrust", 2000))
            {
                WasOpen = true;
                RetainerHandlers.SelectEntrustItems();
            }
        }
        else
        {
            var slot = InventoryManager.Instance()->GetInventoryContainer(type)->GetInventorySlot(i);
            if(type == InventoryType.Crystals)
            {
                RequestEntrustQuantity = (int)toEntrustFromStack;
                CapturedInventoryState = Utils.GetCapturedInventoryState(allowedPlayerInventories);
                EzThrottler.Throttle("InventoryTimeout", 5000, true);
                PluginLog.Information($"Entrusting crystals from slot: {i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)} quantuity = {toEntrustFromStack}");
                P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, RetainerItemCommand.EntrustToRetainer);
            }
            else
            {
                if(item->Quantity <= 1 || item->Quantity == toEntrustFromStack)
                {
                    CapturedInventoryState = Utils.GetCapturedInventoryState(allowedPlayerInventories);
                    EzThrottler.Throttle("InventoryTimeout", 5000, true);
                    PluginLog.Information($"Entrusting from slot: {i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)} quantuity = all");
                    P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, RetainerItemCommand.EntrustToRetainer);
                }
                else
                {
                    //partial entrust
                    RequestEntrustQuantity = (int)toEntrustFromStack;
                    CapturedInventoryState = Utils.GetCapturedInventoryState(allowedPlayerInventories);
                    EzThrottler.Throttle("InventoryTimeout", 5000, true);
                    PluginLog.Information($"Entrusting from slot: {i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)} quantuity = {toEntrustFromStack}");
                    P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, RetainerItemCommand.EntrustQuantity);
                }
            }
        }
    }
}
