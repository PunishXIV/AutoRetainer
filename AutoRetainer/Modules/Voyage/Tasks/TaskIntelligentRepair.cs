using AutoRetainer.Internal;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskIntelligentRepair
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskIntelligentRepair)}, name={name}, type={type}");
        P.TaskManager.Enqueue(() =>
        {
            var rep = VoyageUtils.GetIsVesselNeedsRepair(name, type, out var log);
            if (C.SubsAutoRepair && rep.Count > 0)
            {
                TaskRepairAll.EnqueueImmediate(rep, name, type);
            }
            PluginLog.Debug($"Repair check log: {log.Join(", ")}");
        }, "IntelligentRepairTask");
    }
}
