using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using Dalamud.Memory;
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
            VoyageUtils.Log($"Task enqueued: {nameof(TaskRedeployVessel)} name={name}, type={type}");
            TaskSelectVesselByName.Enqueue(name);
            P.TaskManager.Enqueue(VoyageScheduler.FinalizeVessel);
            P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
            TaskIntelligentRepair.Enqueue(name, type);
            TaskRedeployPreviousLog.Enqueue();
        }
    }
}
