using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskAssignQuickVenture
{
    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        if(P.config.RetainerMenuDelay > 0)
        {
            TaskWaitSelectString.Enqueue(P.config.RetainerMenuDelay);
        }
        P.TaskManager.Enqueue(RetainerHandlers.SelectAssignVenture);
        P.TaskManager.Enqueue(RetainerHandlers.SelectQuickExploration);
        P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
    }
}
