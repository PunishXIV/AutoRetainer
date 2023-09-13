using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskReassignVenture
{
    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        if (C.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(RetainerHandlers.SelectViewVentureReport);
        P.TaskManager.Enqueue(() => RetainerHandlers.EnforceSelectString(RetainerHandlers.SelectViewVentureReport));
        P.TaskManager.Enqueue(RetainerHandlers.ClickResultReassign);
        P.TaskManager.Enqueue(RetainerHandlers.WaitForVentureListUpdate);
        P.TaskManager.DelayNext(C.FrameDelay, true);
        //P.TaskManager.DelayNext(10, true);
        P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
    }
}
