using AutoRetainer.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskIntelligentRepair
    {
        internal static void Enqueue(string name, VoyageType type)
        {
            P.TaskManager.Enqueue(() =>
            {
                var rep = VoyageUtils.IsVesselNeedsRepair(name, type, out var log);
                if (C.SubsAutoRepair && rep.Count > 0)
                {
                    TaskRepairAll.EnqueueImmediate(rep);
                }
                PluginLog.Debug($"Repair check log: {log.Join(", ")}");
            }, "IntelligentRepairTask");
        }
    }
}
