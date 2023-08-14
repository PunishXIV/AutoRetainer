using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal unsafe static class TaskRedeployVessel
    {
        internal static void Enqueue(string name, VoyageType type)
        {
            TaskSelectVesselByName.Enqueue(name);
            P.TaskManager.Enqueue(SchedulerVoyage.FinalizeVessel);
            P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
            P.TaskManager.Enqueue(() =>
            {
                var rep = VoyageUtils.IsVesselNeedsRepair(name, type, out var log);
                if (C.SubsAutoRepair && rep.Count > 0)
                {
                    TaskRepairAll.EnqueueImmediate(rep);
                }
                PluginLog.Debug($"Repair check log: {log.Join(", ")}");
            }, "IntelligentRepairTask");
            P.TaskManager.Enqueue(SchedulerVoyage.SelectViewPreviousLog);
            P.TaskManager.Enqueue(SchedulerVoyage.RedeployVessel);
            P.TaskManager.Enqueue(SchedulerVoyage.DeployVessel);
            P.TaskManager.Enqueue(SchedulerVoyage.WaitForCutscene);
            P.TaskManager.Enqueue(SchedulerVoyage.PressEsc);
            P.TaskManager.Enqueue(SchedulerVoyage.ConfirmSkip);
        }
    }
}
