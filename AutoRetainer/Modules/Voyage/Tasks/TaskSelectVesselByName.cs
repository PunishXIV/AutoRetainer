using AutoRetainer.Internal;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskSelectVesselByName
    {
        internal static void Enqueue(string name, VoyageType type)
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskSelectVesselByName)} ({name})");
            P.TaskManager.Enqueue(() => VoyageScheduler.SelectVesselByName(name, type), $"TaskSelectVesselByName: {name}");
        }
    }
}
