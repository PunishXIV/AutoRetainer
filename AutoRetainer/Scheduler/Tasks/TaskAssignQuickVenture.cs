using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskAssignQuickVenture
{
    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        if(C.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(RetainerHandlers.SelectAssignVenture);
        P.TaskManager.Enqueue(() => RetainerHandlers.EnforceSelectString(RetainerHandlers.SelectAssignVenture));
        P.TaskManager.Enqueue(RetainerHandlers.SelectQuickExploration);
        P.TaskManager.Enqueue(RetainerHandlers.WaitForVentureListUpdate);
        P.TaskManager.DelayNext(C.FrameDelay, true);
        //P.TaskManager.DelayNext(10, true);
        P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
    }
}
