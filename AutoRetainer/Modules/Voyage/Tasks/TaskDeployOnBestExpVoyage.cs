using AutoRetainerAPI.Configuration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static unsafe class TaskDeployOnBestExpVoyage
    {
        internal static void Enqueue(SubmarineUnlockPlan unlock = null)
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskCalculateAndPickBestExpRoute)} (plan: {unlock})");
            P.TaskManager.Enqueue(SelectDeploy);
            TaskCalculateAndPickBestExpRoute.Enqueue(unlock);
            P.TaskManager.Enqueue(Deploy);
            TaskDeployAndSkipCutscene.Enqueue(true);
        }

        internal static bool? SelectDeploy()
        {
            return Utils.TrySelectSpecificEntry(Lang.DeployOnSubaquaticVoyage, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectDeploy", 1000));
        }

        internal static bool? Deploy()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
            {
                var button = addon->UldManager.NodeList[2]->GetAsAtkComponentButton();
                if (!button->IsEnabled)
                {
                    EzThrottler.Throttle("Voyage.Deploy", 500, true);
                    return false;
                }
                else
                {
                    if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Deploy"))
                    {
                        Callback.Fire(addon, true, 0);
                        return true;
                    }
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }
    }
}
