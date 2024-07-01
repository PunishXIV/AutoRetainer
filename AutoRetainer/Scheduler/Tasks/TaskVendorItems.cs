using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

public static class TaskVendorItems
{
    public static void Enqueue()
    {
        P.TaskManager.Enqueue(AddHardItems);
        P.TaskManager.Enqueue(SelectEntrustItems);
        P.TaskManager.Enqueue(WaitUntilInventoryLoaded);
        P.TaskManager.Enqueue(EnqueueImmediateAllItems);
    }

    public static void EnqueueImmediateAllItems()
    {
        if (InventorySpaceManager.SellSlotTasks.Count == 0)
        {
            return;
        }
        else
        {
            foreach (var x in InventorySpaceManager.SellSlotTasks)
            {
                P.TaskManager.EnqueueImmediate(() => InventorySpaceManager.SafeSellSlot(x), $"InventorySpaceManager.SafeSellSlot({x})");
            }
            P.TaskManager.EnqueueImmediate(InventorySpaceManager.SellSlotTasks.Clear);
            P.TaskManager.DelayNextImmediate(333);
            P.TaskManager.EnqueueImmediate(CloseInventory);
        }
    }

    public static bool? CloseInventory()
    {
        return RetainerHandlers.CloseAgentRetainer();
    }

    public static void AddHardItems()
    {
        InventorySpaceManager.EnqueueAllHardItems();
    }

    public static bool? SelectEntrustItems()
    {
        if (InventorySpaceManager.SellSlotTasks.Count == 0)
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
        if (InventorySpaceManager.SellSlotTasks.Count == 0)
        {
            return true;
        }
        else if (InventorySpaceManager.IsRetainerInventoryLoaded())
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
