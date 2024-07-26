using AutoRetainer.Internal;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskEnterMenu
{
    internal static void Enqueue(VoyageType type)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskEnterMenu)} type={type}");
        if(type == VoyageType.Airship)
        {
            P.TaskManager.Enqueue(VoyageScheduler.SelectAirshipManagement);
        }
        else if(type == VoyageType.Submersible)
        {
            P.TaskManager.Enqueue(VoyageScheduler.SelectSubManagement);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
