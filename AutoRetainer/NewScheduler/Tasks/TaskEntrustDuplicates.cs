using AutoRetainer.Multi;
using AutoRetainer.NewScheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.NewScheduler.Tasks
{
    internal static class TaskEntrustDuplicates
    {
        internal static bool NoDuplicates = false;
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(() => { NoDuplicates = false; return true; }) ;
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            P.TaskManager.Enqueue(RetainerHandlers.SelectEntrustItems);
            P.TaskManager.Enqueue(RetainerHandlers.ClickEntrustDuplicates);
            TaskWait.Enqueue(500);
            P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickEntrustDuplicatesConfirm(); }, 600 * 1000, false);
            TaskWait.Enqueue(500);
            P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickCloseEntrustWindow(); }, false);
            P.TaskManager.Enqueue(RetainerHandlers.CloseAgentRetainer);
        }
    }
}
