namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static class TaskDeployOnPreviousVoyage
    {
        internal static void Enqueue()
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskDeployOnPreviousVoyage)}");
            P.TaskManager.Enqueue(VoyageScheduler.SelectViewPreviousLog);
            P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
            TaskDeployAndSkipCutscene.Enqueue();
        }
    }
}
