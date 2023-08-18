using AutoRetainer.Internal;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal unsafe static class TaskFinalizeVessel
    {
        internal static void Enqueue(string name, VoyageType type)
        {
            P.TaskManager.Enqueue(() => VoyageScheduler.SelectVesselByName(name), $"SelectVesselByName = {name}");
            P.TaskManager.Enqueue(VoyageScheduler.FinalizeVessel);
            P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
            if (C.SubsRepairFinalize)
            {
                TaskIntelligentRepair.Enqueue(name, type);
            }
            P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
        }
    }
}
