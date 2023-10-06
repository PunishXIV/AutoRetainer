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
