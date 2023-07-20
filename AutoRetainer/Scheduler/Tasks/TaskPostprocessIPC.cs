using ECommons.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks
{
    internal static class TaskPostprocessIPC
    {
        internal static void Enqueue(string retainer)
        {
            P.TaskManager.Enqueue(() =>
            {
                SchedulerMain.RetainerPostprocess = SchedulerMain.RetainerPostprocess.Clear();
                IPC.FirePostprocessTaskRequestEvent(retainer);
            }, "TaskPostprocessIPCEnqueue");
            P.TaskManager.Enqueue(() =>
            {
                foreach (var x in SchedulerMain.RetainerPostprocess)
                {
                    P.TaskManager.EnqueueImmediate(() =>
                    {
                        SchedulerMain.RetainerPostprocess = SchedulerMain.RetainerPostprocess.Remove(x);
                        SchedulerMain.PostProcessLocked = true;
                        IPC.FirePluginPostprocessEvent(x, retainer);
                    }, "Postprocess request from {x}");
                    P.TaskManager.EnqueueImmediate(() => !SchedulerMain.PostProcessLocked, int.MaxValue, $"Postprocess task from {x}");
                }
            }, "TaskPostprocessProcessEntries");
        }
    }
}
