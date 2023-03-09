using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoRetainer.Multi;
using AutoRetainer.NewScheduler.Handlers;

namespace AutoRetainer.NewScheduler.Tasks
{
    internal static class TaskAssignQuickVenture
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            P.TaskManager.Enqueue(RetainerHandlers.SelectAssignVenture);
            P.TaskManager.Enqueue(RetainerHandlers.SelectQuickExploration);
            P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
        }
    }
}
