using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskWait
{
    internal static void Enqueue(int ms)
    {
        P.TaskManager.Enqueue(() => GenericHandlers.Throttle(ms));
        P.TaskManager.Enqueue(() => GenericHandlers.WaitFor(ms));
    }
}
