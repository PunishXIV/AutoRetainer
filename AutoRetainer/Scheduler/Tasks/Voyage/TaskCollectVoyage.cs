using AutoRetainer.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.Scheduler.Tasks.Voyage
{
    internal static class TaskCollectVoyage
    {
        internal static void Enqueue(string name)
        {
            P.TaskManager.Enqueue(() => SchedulerVoyage.SelectSubjectByName(name));
            P.TaskManager.Enqueue(SchedulerVoyage.Redeploy);
            P.TaskManager.Enqueue(SchedulerVoyage.Deploy);
            P.TaskManager.Enqueue(SchedulerVoyage.WaitForCutscene);
            P.TaskManager.Enqueue(SchedulerVoyage.PressEsc);
            P.TaskManager.Enqueue(SchedulerVoyage.ConfirmSkip);
        }
    }
}
