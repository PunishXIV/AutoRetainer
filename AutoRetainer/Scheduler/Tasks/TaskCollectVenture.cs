using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskCollectVenture
{
    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        if (C.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(RetainerHandlers.SelectViewVentureReport);
        P.TaskManager.Enqueue(() => RetainerHandlers.EnforceSelectString(RetainerHandlers.SelectViewVentureReport), "EnforceSelectString/SelectViewVentureReport");
        P.TaskManager.Enqueue(RetainerHandlers.ClickResultConfirm);
    }
}
