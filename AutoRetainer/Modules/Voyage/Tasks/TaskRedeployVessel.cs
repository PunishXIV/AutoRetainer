using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskRedeployVessel
    {
        internal static void Enqueue(string name, VoyageType type)
        {
            TaskSelectVesselByName.Enqueue(name);
            P.TaskManager.Enqueue(() =>
            {
                if(C.SubsAutoRepair && VoyageUtils.IsVesselNeedsRepair(name, type))
                {
                    P.TaskManager.EnqueueImmediate(SchedulerVoyage.FinalizeVessel);
                    TaskRepairAll.EnqueueImmediate();
                    P.TaskManager.EnqueueImmediate(SchedulerVoyage.SelectViewPreviousLog);
                }
            }, "IntelligentRepairTask");
            P.TaskManager.Enqueue(SchedulerVoyage.RedeployVessel);
            P.TaskManager.Enqueue(SchedulerVoyage.DeployVessel);
            P.TaskManager.Enqueue(SchedulerVoyage.WaitForCutscene);
            P.TaskManager.Enqueue(SchedulerVoyage.PressEsc);
            P.TaskManager.Enqueue(SchedulerVoyage.ConfirmSkip);
        }
    }
}
