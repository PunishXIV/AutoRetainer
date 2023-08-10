using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskFinalizeVessel
    {
        internal static void Enqueue(string name)
        {
            P.TaskManager.Enqueue(() => SchedulerVoyage.SelectVesselByName(name));
            P.TaskManager.Enqueue(SchedulerVoyage.FinalizeVessel);
            P.TaskManager.Enqueue(SchedulerVoyage.SelectVesselQuit);
        }
    }
}
