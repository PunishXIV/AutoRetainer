using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskDeployOnPreviousVoyage
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(SchedulerVoyage.SelectViewPreviousLog);
            P.TaskManager.Enqueue(SchedulerVoyage.RedeployVessel);
            P.TaskManager.Enqueue(SchedulerVoyage.DeployVessel);
            P.TaskManager.Enqueue(SchedulerVoyage.WaitForCutscene);
            P.TaskManager.Enqueue(SchedulerVoyage.PressEsc);
            P.TaskManager.Enqueue(SchedulerVoyage.ConfirmSkip);
        }
    }
}
