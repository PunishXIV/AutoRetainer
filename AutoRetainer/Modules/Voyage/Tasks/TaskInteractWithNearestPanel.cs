using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskInteractWithNearestPanel
    {
        internal static void Enqueue(bool interact = true)
        {
            if (!VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType))
            {
                TaskEnterWorkshop.EnqueueEnterWorkshop();
            }
            P.TaskManager.Enqueue(VoyageScheduler.Lockon);
            P.TaskManager.Enqueue(VoyageScheduler.Approach);
            P.TaskManager.Enqueue(VoyageScheduler.AutomoveOffPanel);
            if(interact) P.TaskManager.Enqueue(VoyageScheduler.InteractWithVoyagePanel);
        }
    }
}
