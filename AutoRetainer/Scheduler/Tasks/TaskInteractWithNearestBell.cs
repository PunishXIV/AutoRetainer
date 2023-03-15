using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal unsafe static class TaskInteractWithNearestBell
{
    internal static void Enqueue()
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        P.TaskManager.Enqueue(PlayerWorldHandlers.SelectNearestBell);
        P.TaskManager.Enqueue(() => { P.IsInteractionAutomatic = true; return true; });
        P.TaskManager.Enqueue(PlayerWorldHandlers.InteractWithTargetedBell);
    }
}
