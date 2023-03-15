using AutoRetainer.Scheduler.Handlers;

namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskWithdrawGil
{
    static bool hasGilInt = false;
    internal static bool forceCheck = false;
    static bool HasGil => hasGilInt || forceCheck;
    internal static void Enqueue(int percent)
    {
        hasGilInt = false;
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        P.TaskManager.Enqueue(() =>
        {
            var g = CurrentRetainerHasGil();
            if (g != null)
            {
                hasGilInt = g.Value;
                return true;
            }
            return false;
        });
        P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.SelectEntrustGil());
        P.TaskManager.Enqueue(() => HasGil == false ? true : GenericHandlers.Throttle(500));
        P.TaskManager.Enqueue(() => HasGil == false ? true : GenericHandlers.WaitFor(500));
        P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.SetWithdrawGilAmount(percent));
        P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.WithdrawGilOrCancel());
        P.TaskManager.Enqueue(() => { forceCheck = false; return true; });
    }

    static bool? CurrentRetainerHasGil()
    {
        if (Utils.TryGetCurrentRetainer(out var name) && Utils.TryGetRetainerByName(name, out var ret))
        {
            return ret.Gil > 0;
        }
        return null;
    }
}
