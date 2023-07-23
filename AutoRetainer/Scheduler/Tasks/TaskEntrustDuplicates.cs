using AutoRetainer.Scheduler.Handlers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskEntrustDuplicates
{
    internal static bool NoDuplicates = false;

    internal static unsafe bool CheckNoDuplicates() {
        for (var rI = InventoryType.RetainerPage1; rI <= InventoryType.RetainerPage7; rI++) {
            var inv = InventoryManager.Instance()->GetInventoryContainer(rI);
            if (inv == null || inv->Loaded == 0) continue;
            for (var slot = 0; slot < inv->Size; slot++) {
                var slotItem = inv->GetInventorySlot(slot);
                if (slotItem == null) continue;
                if (InventoryManager.Instance()->GetInventoryItemCount(slotItem->ItemID, slotItem->Flags.HasFlag(InventoryItem.ItemFlags.HQ)) > 0) {
                    return false;
                }
            }
        }
        return true;
    }

    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(() => { NoDuplicates = CheckNoDuplicates(); return true; }) ;
        P.TaskManager.Enqueue(() => { NoDuplicates = false; return true; }) ;
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        if (C.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.SelectEntrustItems(); });
        P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickEntrustDuplicates(); });
        TaskWait.Enqueue(500);
        P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickEntrustDuplicatesConfirm(); }, 600 * 1000, false);
        TaskWait.Enqueue(500);
        P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickCloseEntrustWindow(); }, false);
        P.TaskManager.Enqueue(RetainerHandlers.CloseAgentRetainer);
    }
}
