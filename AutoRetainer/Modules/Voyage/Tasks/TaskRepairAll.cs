using ECommons.Throttlers;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal unsafe static class TaskRepairAll
    {
        internal static void EnqueueImmediate(List<int> indexes)
        {
            P.TaskManager.EnqueueImmediate(() => Utils.TrySelectSpecificEntry(new string[] { "Repair submersible components" , "Repair airship components"}, () => EzThrottler.Throttle("RepairAllSelectRepair")), "RepairAllSelectRepair");
            foreach(int index in indexes)
            {
                if(index < 0 || index > 3) throw new ArgumentOutOfRangeException(nameof(index));
                P.TaskManager.EnqueueImmediate(() => SchedulerVoyage.TryRepair(index), $"Repair {index}");
                P.TaskManager.EnqueueImmediate(SchedulerVoyage.ConfirmRepair, 5000, false);
            }
            P.TaskManager.EnqueueImmediate(SchedulerVoyage.CloseRepair);
        }
    }
}
