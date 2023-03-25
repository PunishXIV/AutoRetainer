using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskSelectRetainer
{
    internal static void Enqueue(string name)
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(name));
        P.TaskManager.Enqueue(() => Utils.TryGetCurrentRetainer(out _));
    }
}
