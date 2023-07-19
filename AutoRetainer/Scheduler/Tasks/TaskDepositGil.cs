using AutoRetainer.Scheduler.Handlers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Scheduler.Tasks;

internal unsafe static class TaskDepositGil
{
    internal static bool forceCheck = false;
    static bool HasGil => Gil > 0 || forceCheck;
    internal static int Gil => InventoryManager.Instance()->GetInventoryItemCount(1);
    internal static void Enqueue(int percent)
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        if (C.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.SelectEntrustGil());
        P.TaskManager.Enqueue(() => HasGil == false ? true : GenericHandlers.Throttle(500));
        P.TaskManager.Enqueue(() => HasGil == false ? true : GenericHandlers.WaitFor(500));
        P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.SwapBankMode());
        P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.SetDepositGilAmount(percent));
        P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.ProcessBankOrCancel());
        P.TaskManager.Enqueue(() => { forceCheck = false; return true; });
    }
}
