using AutoRetainer.Multi;
using AutoRetainer.NewScheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.NewScheduler.Tasks
{
    internal static class TaskCollectVenture
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            P.TaskManager.Enqueue(RetainerHandlers.SelectViewVentureReport);
            P.TaskManager.Enqueue(RetainerHandlers.ClickResultReassign);
            P.TaskManager.Enqueue(RetainerHandlers.ClickAskReturn);
        }
    }
}
