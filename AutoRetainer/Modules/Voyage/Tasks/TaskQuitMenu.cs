using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoRetainer.Modules.Voyage;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskQuitMenu
    {
        internal static void Enqueue()
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskQuitMenu)}");
            P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselSelectorMenu);
        }
    }
}
