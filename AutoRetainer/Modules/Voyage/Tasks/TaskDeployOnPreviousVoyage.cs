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
            P.TaskManager.Enqueue(VoyageScheduler.SelectViewPreviousLog);
            P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
            TaskDeployAndSkipCutscene.Enqueue();
        }
    }
}
