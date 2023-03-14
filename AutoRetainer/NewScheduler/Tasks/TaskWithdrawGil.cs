using AutoRetainer.Multi;
using AutoRetainer.NewScheduler.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.NewScheduler.Tasks
{
    internal static class TaskWithdrawGil
    {
        static bool HasGil = false;
        internal static void Enqueue(int percent)
        {
            HasGil = false;
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            P.TaskManager.Enqueue(() =>
            {
                var g = CurrentRetainerHasGil();
                if (g != null)
                {
                    HasGil = g.Value;
                    return true;
                }
                return false;
            });
            P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.SelectEntrustGil());
            P.TaskManager.Enqueue(() => HasGil == false ? true : GenericHandlers.Throttle(500));
            P.TaskManager.Enqueue(() => HasGil == false ? true : GenericHandlers.WaitFor(500));
            P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.SetWithdrawGilAmount(percent));
            P.TaskManager.Enqueue(() => HasGil == false ? true : RetainerHandlers.WithdrawGilOrCancel());
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
}
