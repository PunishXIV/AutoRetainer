using ECommons.Throttlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal unsafe static class TaskRepairAll
    {
        internal static void EnqueueImmediate()
        {
            P.TaskManager.EnqueueImmediate(() => Utils.TrySelectSpecificEntry(new string[] { "Repair submersible components" , "Repair airship components"}, () => EzThrottler.Throttle("RepairAllSelectRepair")), "RepairAllSelectRepair");
            P.TaskManager.EnqueueImmediate(() => SchedulerVoyage.TryRepair(0), "Repair 0");
            P.TaskManager.EnqueueImmediate(SchedulerVoyage.ConfirmRepair, 1000, false);
            P.TaskManager.EnqueueImmediate(() => SchedulerVoyage.TryRepair(1), "Repair 1");
            P.TaskManager.EnqueueImmediate(SchedulerVoyage.ConfirmRepair, 1000, false);
            P.TaskManager.EnqueueImmediate(() => SchedulerVoyage.TryRepair(2), "Repair 2");
            P.TaskManager.EnqueueImmediate(SchedulerVoyage.ConfirmRepair, 1000, false);
            P.TaskManager.EnqueueImmediate(() => SchedulerVoyage.TryRepair(3), "Repair 3");
            P.TaskManager.EnqueueImmediate(SchedulerVoyage.ConfirmRepair, 1000, false);
            P.TaskManager.EnqueueImmediate(SchedulerVoyage.CloseRepair);
        }
    }
}
