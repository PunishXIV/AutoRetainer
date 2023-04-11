using AutoRetainer.Scheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks
{
    internal class TaskAssignFieldExploration
    {
        internal static void Enqueue(uint VentureID)
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            if (P.config.RetainerMenuDelay > 0)
            {
                TaskWaitSelectString.Enqueue(P.config.RetainerMenuDelay);
            }
            P.TaskManager.Enqueue(RetainerHandlers.SelectAssignVenture);
            P.TaskManager.Enqueue(() => RetainerHandlers.GenericSelectByName(Consts.FieldExplorationNames));
            P.TaskManager.Enqueue(() => RetainerHandlers.SelectSpecificVenture(VentureID));
            P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
        }
    }
}
