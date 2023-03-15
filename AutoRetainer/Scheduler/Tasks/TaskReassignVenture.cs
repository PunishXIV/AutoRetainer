using AutoRetainer.Helpers;
using AutoRetainer.Scheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks
{
    internal static class TaskReassignVenture
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            P.TaskManager.Enqueue(RetainerHandlers.SelectViewVentureReport);
            P.TaskManager.Enqueue(RetainerHandlers.ClickResultReassign);
            P.TaskManager.Enqueue(RetainerHandlers.ClickAskAssign);
        }
    }
}
