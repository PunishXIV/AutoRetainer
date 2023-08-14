using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskSelectVesselByName
    {
        internal static void Enqueue(string name)
        {
            P.TaskManager.Enqueue(() => VoyageScheduler.SelectVesselByName(name), $"TaskSelectVesselByName: {name}");
        }
    }
}
