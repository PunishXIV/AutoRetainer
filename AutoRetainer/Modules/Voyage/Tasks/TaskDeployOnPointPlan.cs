using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal unsafe static class TaskDeployOnPointPlan
    {
        internal static void Enqueue(SubmarinePointPlan unlock)
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskDeployOnPointPlan)} (plan: {unlock})");

            P.TaskManager.Enqueue(TaskDeployOnBestExpVoyage.SelectDeploy);
            EnqueuePick(unlock);
            P.TaskManager.Enqueue(TaskDeployOnBestExpVoyage.Deploy);
            TaskDeployAndSkipCutscene.Enqueue(true);
        }
        internal static void EnqueuePick(SubmarinePointPlan unlock)
        {
            P.TaskManager.Enqueue(() => PickFromPlan(unlock), $"PickFromPlan({unlock})");
        }

        internal static void PickFromPlan(SubmarinePointPlan unlock)
        {
            var points = unlock.Points;
            VoyageUtils.Log($"points: {points.Select(x => $"{x}").Join("\n")}");
            TaskPickSubmarineRoute.EnqueueImmediate(unlock.GetMapId(), points.ToArray());
        }
    }
}
