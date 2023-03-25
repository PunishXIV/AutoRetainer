using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskReassignVenture
{
    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        if (P.config.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(P.config.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(RetainerHandlers.SelectViewVentureReport);
        P.TaskManager.Enqueue(RetainerHandlers.ClickResultReassign);
        P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
    }
}
