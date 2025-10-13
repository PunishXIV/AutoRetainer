using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Handlers;
using ECommons.GameHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Scheduler.Tasks;

public static class TaskVendorItems
{
    public static void EnqueueFromCommand()
    {
        if(NpcSaleManager.GetValidNPC() != null && Data.GetIMSettings().IMEnableNpcSell)
        {
            NpcSaleManager.EnqueueIfItemsPresent(true);
        }
        else if(HasVendorableItems() && Data.GetIMSettings().IMEnableAutoVendor && Utils.GetReachableRetainerBell(true) != null && Player.IsInHomeWorld && Data.RetainerData.Count > 0)
        {
            P.SkipNextEnable = true;
            P.TaskManager.Enqueue(() => !IsOccupied());
            TaskInteractWithNearestBell.Enqueue(true);
            P.TaskManager.Enqueue(() => TryGetAddonMaster<AddonMaster.RetainerList>(out var m) && m.IsAddonReady);
            P.TaskManager.Enqueue(() =>
            {
                P.TaskManager.InsertStack(Utils.EnqueueVendorItemsByRetainer);
            });
            P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
        }
    }

    public unsafe static bool HasVendorableItems()
    {
        foreach(var type in InventorySpaceManager.GetAllowedToSellInventoryTypes())
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(type);
            if(inv != null)
            {
                for(var i = 0; i < inv->Size; i++)
                {
                    var slot = inv->GetInventorySlot(i);
                    if(slot != null && slot->ItemId != 0)
                    {
                        if(Utils.IsItemSellableByHardList(slot->ItemId, slot->Quantity))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public static void Enqueue(bool softAsHard = false)
    {
        P.TaskManager.Enqueue(() => AddHardItems(softAsHard));
        P.TaskManager.Enqueue(SelectEntrustItems);
        P.TaskManager.Enqueue(WaitUntilInventoryLoaded);
        P.TaskManager.Enqueue(EnqueueImmediateAllItems);
    }

    public static void EnqueueImmediateAllItems()
    {
        if(InventorySpaceManager.SellSlotTasks.Count == 0)
        {
            return;
        }
        else
        {
            P.TaskManager.BeginStack();
            try
            {
                foreach(var x in InventorySpaceManager.SellSlotTasks)
                {
                    P.TaskManager.Enqueue(() => InventorySpaceManager.SafeSellSlot(x), $"InventorySpaceManager.SafeSellSlot({x})");
                }
                P.TaskManager.Enqueue(InventorySpaceManager.SellSlotTasks.Clear);
                P.TaskManager.EnqueueDelay(333);
                P.TaskManager.Enqueue(CloseInventory);
            }
            catch(Exception e) { e.Log(); }
            P.TaskManager.InsertStack();
        }
    }

    public static bool? CloseInventory()
    {
        return RetainerHandlers.CloseAgentRetainer();
    }

    public static void AddHardItems(bool softAsHard = false)
    {
        InventorySpaceManager.EnqueueAllHardItems(softAsHard);
    }

    public static bool? SelectEntrustItems()
    {
        if(InventorySpaceManager.SellSlotTasks.Count == 0)
        {
            return true;
        }
        else
        {
            return RetainerHandlers.SelectEntrustItems();
        }
    }

    public static bool? WaitUntilInventoryLoaded()
    {
        if(InventorySpaceManager.SellSlotTasks.Count == 0)
        {
            return true;
        }
        else if(InventorySpaceManager.IsRetainerInventoryLoaded())
        {
            return true;
        }
        else
        {
            Utils.RethrottleGeneric();
            return false;
        }
    }
}
