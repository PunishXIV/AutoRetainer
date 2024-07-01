using AutoRetainer.Internal;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskDeployOnPreviousVoyage
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskDeployOnPreviousVoyage)}");
        TaskIntelligentRepair.Enqueue(name, type);
        P.TaskManager.Enqueue(VoyageScheduler.SelectViewPreviousLog);
        P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
        TaskDeployAndSkipCutscene.Enqueue();
    }
}
