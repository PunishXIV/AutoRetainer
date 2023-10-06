using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks
{
    internal class TaskAssignFieldExploration
    {
        internal static void Enqueue(uint VentureID)
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            if (C.RetainerMenuDelay > 0)
            {
                TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
            }
            P.TaskManager.Enqueue(RetainerHandlers.SelectAssignVenture);
            P.TaskManager.Enqueue(() => RetainerHandlers.GenericSelectByName(Lang.FieldExplorationNames), "GenericSelectByName");
            P.TaskManager.Enqueue(RetainerHandlers.WaitForVentureListUpdate);
            P.TaskManager.DelayNext(C.FrameDelay, true);
            P.TaskManager.Enqueue(() => RetainerHandlers.SelectSpecificVenture(VentureID), "SelectSpecificVenture");
            //P.TaskManager.DelayNext(10, true);
            //P.TaskManager.Enqueue(() => RetainerHandlers.CheckForErrorAssignedVenture(VentureID), 500, false, "FirstErrorCheck");
            P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
        }
    }
}
