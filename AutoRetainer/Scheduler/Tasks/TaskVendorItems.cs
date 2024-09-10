using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

public static class TaskVendorItems
{
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
