using AutoRetainer.Internal;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal unsafe static class TaskRedeployVessel
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskRedeployVessel)} name={name}, type={type}");
        TaskSelectVesselByName.Enqueue(name, type);
        P.TaskManager.Enqueue(VoyageScheduler.FinalizeVessel);
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
        TaskIntelligentRepair.Enqueue(name, type);
        TaskRedeployPreviousLog.Enqueue();
    }
}
