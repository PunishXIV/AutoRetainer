using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Handlers;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterCombinedImplementations;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Scheduler.Tasks;

internal static unsafe class TaskEntrustDuplicates
{
    internal static int RequestEntrustQuantity = 0;
    internal static List<(uint ID, uint Quantity)> CapturedInventoryState = [];
    internal static bool WasOpen = false;

    public static void EnqueueNew(EntrustPlan plan)
    {
        P.TaskManager.Enqueue((System.Action)(() => WasOpen = false), "Set WasOpen = false");
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "Wait until addon SelectString ready");
        P.TaskManager.Enqueue(() => RecursivelyEntrustItems(plan), $"Recursivelty entrust items ({plan.Guid} | {plan.Name})", new(timeLimitMS: 60 * 60 * 1000));
        P.TaskManager.Enqueue(() => !WasOpen || TaskVendorItems.CloseInventory() == true);
    }
    public static void EnqueueNewReverse(EntrustPlan plan)
    {
        P.TaskManager.Enqueue((System.Action)(() => WasOpen = false), "Set WasOpen = false");
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "Wait until addon SelectString ready");
        P.TaskManager.Enqueue(() => RecursivelyReverseEntrustItems(plan), $"Recursivelty reverse entrust items ({plan.Guid} | {plan.Name})", new(timeLimitMS: 60 * 60 * 1000));
        P.TaskManager.Enqueue(() => !WasOpen || TaskVendorItems.CloseInventory() == true);
    }

    private static bool? RecursivelyEntrustItems(EntrustPlan plan)
    {
        try
        {
            var invComb = AddonMasterCombined.ICombinedInventory.Get();
            if(invComb != null)
            {
                if(EzThrottler.Throttle("openArmory")) invComb.OpenArmoryChest();
            }
            var s = Data.GetIMSettings();
            var allowedPlayerInventories = plan.GetAllowedInventories();
            if(TryGetAddonByName<AtkUnitBase>("InputNumeric", out var numeric))
            {
                if(IsAddonReady(numeric))
                {
                    var maxAmount = numeric->AtkValues[3].UInt;
                    var result = Math.Clamp(RequestEntrustQuantity, 1, maxAmount);
                    if(EzThrottler.Throttle("EntrustItemInputNumeric", 200))
                    {
                        InternalLog.Information($"Processing input numeric: {result} (max: {maxAmount})");
                        Callback.Fire(numeric, true, (int)result);
                    }
                }
                return false;
            }
            if(!EzThrottler.Check("InventoryTimeout") && Utils.GetCapturedInventoryState(allowedPlayerInventories).SequenceEqual(CapturedInventoryState))
            {
                return false;
            }
            if(EzThrottler.Check("EntrustItem") && EzThrottler.Throttle("EntrustItem", Utils.GenerateRandomDelay()))
            {
                List<(uint ItemID, int ToKeep)> itemList = [];
                foreach(var x in plan.EntrustItems)
                {
                    var add = (x, plan.EntrustItemsAmountToKeep.SafeSelect(x));
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(add.Item1)) continue;
                    if(S.CabinetManager.ShouldExcludeItemFromProcessing(add.Item1)) continue;
                    itemList.Add(add);
                    InternalLog.Debug($"[TED] From EntrustItems added item: {ExcelItemHelper.GetName(add.Item1, true)} toKeep={add.Item2}");
                }
                foreach(var x in Utils.GetItemsInInventory(allowedPlayerInventories))
                {
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(x)) continue;
                    if(S.CabinetManager.ShouldExcludeItemFromProcessing(x)) continue;
                    var item = ExcelItemHelper.Get(x);
                    if(item == null) continue;
                    if(itemList.Any(s => s.ItemID == item?.RowId)) continue;
                    if(plan.EntrustCategories.TryGetFirst(c => c.ID == item.Value.ItemUICategory.RowId, out var info))
                    {
                        var add = (item.Value.RowId, info.AmountToKeep);
                        itemList.Add(add);
                        InternalLog.Debug($"[TED] From EntrustCategories added item: {ExcelItemHelper.GetName(add.Item1, true)} toKeep={add.Item2}");
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
                                if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                                if(S.CabinetManager.ShouldExcludeItemFromProcessing(item->ItemId)) continue;
                                if(itemList.Any(s => s.ItemID == item->ItemId)) continue;
                                var data = ExcelItemHelper.Get(item->ItemId);
                                itemList.Add((item->ItemId, 0));
                                InternalLog.Debug($"[TED] From retainer multistack duplicate added: {ExcelItemHelper.GetName(item->ItemId, true)}");
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
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            if(S.CabinetManager.ShouldExcludeItemFromProcessing(item->ItemId)) continue;
                            var itemCount = Utils.GetItemCount(allowedPlayerInventories, item->ItemId);
                            InternalLog.Debug($"[TED] Item count for {ExcelItemHelper.GetName(item->ItemId, true)} = {itemCount}");
                            var data = ExcelItemHelper.Get(item->ItemId);
                            if(itemList.TryGetFirst(s => s.ItemID == item->ItemId, out var entrustInfo))
                            {
                                var toKeep = entrustInfo.ToKeep;
                                var toEntrust = itemCount - toKeep;
                                var canFit = Utils.GetAmountThatCanFit(Utils.RetainerInventoriesWithCrystals, item->ItemId, item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality), out var debugData);
                                InternalLog.Debug($"[TED] For {ExcelItemHelper.GetName(item->ItemId, true)} toEntrust={toEntrust}, toKeep={toKeep}, canFit={canFit}\n{debugData.Print("\n")}");
                                if(toEntrust > canFit) toEntrust = (int)canFit;
                                if(toEntrust > 0)
                                {
                                    var toEntrustFromStack = Math.Min(item->Quantity, toEntrust);
                                    if(toEntrustFromStack > 0)
                                    {
                                        MoveSlotFromToRetainerInventoryUnsafe(item, (int)toEntrustFromStack, i, type, allowedPlayerInventories);
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
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            if(S.CabinetManager.ShouldExcludeItemFromProcessing(item->ItemId)) continue;
                            if(item->ItemId != 0 && !itemList.Any(s => s.ItemID == item->ItemId))
                            {
                                var data = ExcelItemHelper.Get(item->ItemId);
                                if(data == null || data.Value.IsUnique) continue;
                                var canFit = data.Value.StackSize - item->Quantity;
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
                                                MoveSlotFromToRetainerInventoryUnsafe(playerItem, (int)toEntrustFromStack, q, playerType, allowedPlayerInventories);
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

    private static bool? RecursivelyReverseEntrustItems(EntrustPlan plan)
    {
        try
        {
            var s = Data.GetIMSettings();
            var allowedRetainerInventories = Utils.RetainerInventoriesWithCrystals;
            if(TryGetAddonByName<AtkUnitBase>("InputNumeric", out var numeric))
            {
                if(IsAddonReady(numeric))
                {
                    var maxAmount = numeric->AtkValues[3].UInt;
                    var result = Math.Clamp(RequestEntrustQuantity, 1, maxAmount);
                    if(EzThrottler.Throttle("EntrustItemInputNumeric", 200))
                    {
                        InternalLog.Information($"Processing input numeric: {result} (max: {maxAmount})");
                        Callback.Fire(numeric, true, (int)result);
                    }
                }
                return false;
            }
            if(!EzThrottler.Check("InventoryTimeout") && Utils.GetCapturedInventoryState(allowedRetainerInventories).SequenceEqual(CapturedInventoryState))
            {
                return false;
            }
            if(EzThrottler.Check("EntrustItem") && EzThrottler.Throttle("EntrustItem", Utils.GenerateRandomDelay()))
            {
                List<(uint ItemID, int ToKeep)> itemList = [];
                foreach(var x in plan.EntrustItems)
                {
                    var add = (x, plan.EntrustItemsAmountToKeep.SafeSelect(x));
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(add.Item1)) continue;
                    itemList.Add(add);
                    InternalLog.Debug($"[TED] From EntrustItems added item: {ExcelItemHelper.GetName(add.Item1, true)} toKeep={add.Item2}");
                }
                foreach(var x in Utils.GetItemsInInventory(allowedRetainerInventories))
                {
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(x)) continue;
                    var item = ExcelItemHelper.Get(x);
                    if(item == null) continue;
                    if(itemList.Any(s => s.ItemID == item?.RowId)) continue;
                    if(plan.EntrustCategories.TryGetFirst(c => c.ID == item.Value.ItemUICategory.RowId, out var info))
                    {
                        var add = (item.Value.RowId, info.AmountToKeep);
                        itemList.Add(add);
                        InternalLog.Debug($"[TED] From EntrustCategories added item: {ExcelItemHelper.GetName(add.Item1, true)} toKeep={add.Item2}");
                    }
                }
                if(plan.Duplicates && plan.DuplicatesMultiStack)
                {
                    foreach(var type in (InventoryType[])[..Utils.PlayerArmory, ..Utils.PlayerInvetoriesWithCrystals])
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                            if(item->ItemId != 0 && item->Quantity > 0)
                            {
                                if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                                if(itemList.Any(s => s.ItemID == item->ItemId)) continue;
                                var data = ExcelItemHelper.Get(item->ItemId);
                                itemList.Add((item->ItemId, 0));
                                InternalLog.Debug($"[TED] From retainer multistack duplicate added: {ExcelItemHelper.GetName(item->ItemId, true)}");
                            }
                        }
                    }
                }
                //processing unconditional entrusts
                foreach(var type in allowedRetainerInventories)
                {
                    var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                    for(var i = 0; i < inv->Size; i++)
                    {
                        var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                        if(item->ItemId != 0 && item->Quantity > 0)
                        {
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            var itemCount = Utils.GetItemCount(allowedRetainerInventories, item->ItemId);
                            InternalLog.Debug($"[TED] Item count for {ExcelItemHelper.GetName(item->ItemId, true)} = {itemCount}");
                            var data = ExcelItemHelper.Get(item->ItemId);
                            if(itemList.TryGetFirst(s => s.ItemID == item->ItemId, out var entrustInfo))
                            {
                                var toKeep = entrustInfo.ToKeep;
                                var toEntrust = itemCount - toKeep;
                                var canFit = Utils.GetAmountThatCanFit(Utils.PlayerInvetoriesWithCrystals, item->ItemId, item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality), out var debugData);
                                InternalLog.Debug($"[TED] For {ExcelItemHelper.GetName(item->ItemId, true)} toEntrust={toEntrust}, toKeep={toKeep}, canFit={canFit}\n{debugData.Print("\n")}");
                                if(toEntrust > canFit) toEntrust = (int)canFit;
                                if(toEntrust > 0)
                                {
                                    var toEntrustFromStack = Math.Min(item->Quantity, toEntrust);
                                    if(toEntrustFromStack > 0)
                                    {
                                        MoveSlotFromToRetainerInventoryUnsafe(item, (int)toEntrustFromStack, i, type, allowedRetainerInventories);
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
                    foreach(var type in (InventoryType[])[.. Utils.PlayerArmory, .. Utils.PlayerInvetoriesWithCrystals])
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        //find incomplete stacks, then query them from player inventory
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = inv->GetInventorySlot(i);
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            if(item->ItemId != 0 && !itemList.Any(s => s.ItemID == item->ItemId))
                            {
                                var data = ExcelItemHelper.Get(item->ItemId);
                                if(data == null || data.Value.IsUnique) continue;
                                var canFit = data.Value.StackSize - item->Quantity;
                                if(canFit > 0)
                                {
                                    foreach(var playerType in allowedRetainerInventories)
                                    {
                                        var playerInv = InventoryManager.Instance()->GetInventoryContainer(playerType);
                                        for(var q = 0; q < playerInv->Size; q++)
                                        {
                                            var playerItem = playerInv->GetInventorySlot(q);
                                            if(playerItem->ItemId == item->ItemId && playerItem->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality) == item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality))
                                            {
                                                var toEntrustFromStack = Math.Min(canFit, playerItem->Quantity);
                                                MoveSlotFromToRetainerInventoryUnsafe(playerItem, (int)toEntrustFromStack, q, playerType, allowedRetainerInventories);
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
    private static void MoveSlotFromToRetainerInventoryUnsafe(InventoryItem* item, int toEntrustFromStack, int i, InventoryType type, InventoryType[] allowedSourceInventories)
    {
        RetainerItemCommand command;
        RetainerItemCommand quantityCommand;
        if(type.EqualsAny([..Utils.PlayerInvetoriesWithCrystals, .. Utils.PlayerArmory]))
        {
            command = RetainerItemCommand.EntrustToRetainer;
            quantityCommand = RetainerItemCommand.EntrustQuantity;
        }
        else if(type.EqualsAny(Utils.RetainerEntireInventory))
        {
            command = RetainerItemCommand.RetrieveFromRetainer;
            quantityCommand = RetainerItemCommand.RetrieveQuantity;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }
        if(!InventorySpaceManager.IsRetainerInventoryLoaded())
        {
            if(EzThrottler.Throttle("REI SelectEntrust", 2000))
            {
                DebugLog($"SelectEntrust triggered");
                WasOpen = true;
                RetainerHandlers.SelectEntrustItems();
            }
        }
        else
        {
            var slot = InventoryManager.Instance()->GetInventoryContainer(type)->GetInventorySlot(i);
            void printToChat()
            {
                if(C.EnableEntrustChat && ExcelItemHelper.Get(slot->ItemId) != null) Svc.Chat.Print(new SeStringBuilder().Append("Entrusting: ").Append([new ItemPayload(slot->ItemId, slot->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality))]).Append(ExcelItemHelper.GetName(slot->ItemId)).Append([RawPayload.LinkTerminator]).Build());
            }
            if(type == InventoryType.Crystals || type == InventoryType.RetainerCrystals)
            {
                RequestEntrustQuantity = (int)toEntrustFromStack;
                CapturedInventoryState = Utils.GetCapturedInventoryState(allowedSourceInventories);
                EzThrottler.Throttle("InventoryTimeout", 5000, true);
                InternalLog.Debug($"Entrusting crystals from slot: {i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)} quantuity = {toEntrustFromStack}");
                printToChat();
                P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, command);
            }
            else
            {
                if(item->Quantity <= 1 || item->Quantity == toEntrustFromStack)
                {
                    CapturedInventoryState = Utils.GetCapturedInventoryState(allowedSourceInventories);
                    EzThrottler.Throttle("InventoryTimeout", 5000, true);
                    InternalLog.Debug($"Entrusting from slot: {i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)} quantuity = all");
                    printToChat();
                    P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, command);
                }
                else
                {
                    //partial entrust
                    RequestEntrustQuantity = (int)toEntrustFromStack;
                    CapturedInventoryState = Utils.GetCapturedInventoryState(allowedSourceInventories);
                    EzThrottler.Throttle("InventoryTimeout", 5000, true);
                    InternalLog.Debug($"Entrusting from slot: {i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)} quantuity = {toEntrustFromStack}");
                    printToChat();
                    P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, quantityCommand);
                }
            }
        }
    }
}
