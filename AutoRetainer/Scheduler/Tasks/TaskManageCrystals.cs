using AutoRetainer.Scheduler.Handlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Scheduler.Tasks;

internal unsafe class TaskManageCrystals
{
    internal static List<uint> CrystalIDs = Enumerable.Range(2, 18).Select(x => (uint)x).ToList();

    internal static int RetainerQuantity = 0;
    internal enum TransactionType
    {
        Deposit, Withdraw
    }
    public static bool InventoryHasMaxCrystals(int maxAmount)
    {
        List<int> inventoryQuantity = new List<int>();
        foreach (var id in CrystalIDs)
        {
            inventoryQuantity.Add(InventoryManager.Instance()->GetInventoryItemCount(id));
        }

        if (inventoryQuantity.All(x => x == maxAmount))
            return true;

        return false;
    }

    public static bool InventoryHasOverMaxCrystals(int maxAmount)
    {
        List<int> inventoryQuantity = new List<int>();
        foreach (var id in CrystalIDs)
        {
            inventoryQuantity.Add(InventoryManager.Instance()->GetInventoryItemCount(id));
        }

        if (inventoryQuantity.Any(x => x > maxAmount))
            return true;

        return false;
    }

    public static bool InventoryHasUnderMaxCrystals(int maxAmount)
    {
        List<int> inventoryQuantity = new List<int>();
        foreach (var id in CrystalIDs)
        {
            inventoryQuantity.Add(InventoryManager.Instance()->GetInventoryItemCount(id));
        }

        if (inventoryQuantity.Any(x => x < maxAmount))
            return true;

        return false;
    }

    internal static void Enqueue(int maxAmount, TransactionType transactionType)
    {
        if (transactionType == TransactionType.Deposit && InventoryHasOverMaxCrystals(maxAmount) ||
            transactionType == TransactionType.Withdraw && InventoryHasUnderMaxCrystals(maxAmount))
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            if (P.config.RetainerMenuDelay > 0)
            {
                TaskWaitSelectString.Enqueue(P.config.RetainerMenuDelay);
            }
            P.TaskManager.Enqueue(() => RetainerHandlers.SelectEntrustItems(), "OpenEntrustItems");
            P.TaskManager.DelayNext("CrystalThrottler", 200);
            if (transactionType == TransactionType.Withdraw)
            {
                foreach (var id in CrystalIDs)
                {
                    P.TaskManager.Enqueue(() => CheckIfUnderAndWithdraw(id, maxAmount), "CheckIfUnderAndWithdraw");
                }
            }
            else
            {
                foreach (var id in CrystalIDs)
                {
                    P.TaskManager.Enqueue(() => CheckIfOverAndDeposit(id, maxAmount));
                }
            }
            P.TaskManager.Enqueue(() => RetainerHandlers.CloseAgentRetainer());

        }
    }

    private static bool? CheckIfUnderAndWithdraw(uint id, int maxAmount)
    {
        var inventoryAmount = InventoryManager.Instance()->GetInventoryItemCount(id);
        if (inventoryAmount >= maxAmount || inventoryAmount == 9999)
            return true;

        var difference = maxAmount - inventoryAmount;
        P.TaskManager.EnqueueImmediate(() => RetainerHandlers.OpenWithdrawItemMenu(id, out RetainerQuantity));
        P.TaskManager.EnqueueImmediate(() => { difference = Math.Min(difference, RetainerQuantity); });
        P.TaskManager.DelayNextImmediate("MenuOpenThrottle", 400);
        P.TaskManager.EnqueueImmediate(() => RetainerHandlers.ConfirmNumericInputValue(difference));
        P.TaskManager.DelayNextImmediate("DelayCheckingNext", 400);

        return true;
    }

    private static bool? CheckIfOverAndDeposit(uint id, int maxAmount)
    {
        var inventoryAmount = InventoryManager.Instance()->GetInventoryItemCount(id);
        if (inventoryAmount <= maxAmount || inventoryAmount == 0)
            return true;

        var difference = inventoryAmount - maxAmount;
        P.TaskManager.EnqueueImmediate(() => CheckRetainerCrystalCount(id, out RetainerQuantity));
        P.TaskManager.EnqueueImmediate(() =>
        {
            if (RetainerQuantity + difference > 9999)
            {
                var newDiff = (RetainerQuantity + difference) - 9999;
                difference = difference - newDiff;
            }
        });
        P.TaskManager.EnqueueImmediate(() => difference <= 0 ? true : OpenCrystalDepositMenu(id));
        P.TaskManager.DelayNextImmediate("MenuOpenThrottle", 200);
        P.TaskManager.EnqueueImmediate(() => difference <= 0 ? true : RetainerHandlers.ConfirmNumericInputValue(difference));
        P.TaskManager.DelayNextImmediate("DelayCheckingNext", 200);

        return true;
    }

    internal static bool? OpenCrystalDepositMenu(uint itemId)
    {
        var container = InventoryManager.Instance()->GetInventoryContainer(InventoryType.Crystals);
        for (int i = 0; i < container->Size; i++)
        {
            var item = container->GetInventorySlot(i);
            if (item->ItemID == itemId)
            {
                var ag = AgentInventoryContext.Instance();
                ag->OpenForItemSlot(item->Container, i, AgentModule.Instance()->GetAgentByInternalId(AgentId.Retainer)->GetAddonID());
                var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextMenu", 1);
                if (contextMenu != null)
                {
                    if (item->Quantity == 1 || item->ItemID <= 19)
                    {
                        Callback(contextMenu, 0, 0, 0, 0, 0);
                    }
                    return true;
                }
            }
        }

        return true;
    }

    static bool? CheckRetainerCrystalCount(uint itemId, out int quantity)
    {
        quantity = 0;
        if (Utils.TryGetCurrentRetainer(out var name) && Utils.TryGetRetainerByName(name, out var ret))
        {
            var crystalContainer = InventoryManager.Instance()->GetInventoryContainer(InventoryType.RetainerCrystals);
            for (int i = 0; i < crystalContainer->Size; i++)
            {
                var crystal = crystalContainer->GetInventorySlot(i);
                if (crystal->ItemID == itemId)
                {
                    quantity = (int)crystal->Quantity;
                    if (quantity > 0)
                        return true;
                }
            }
        }
        return true;
    }
}

