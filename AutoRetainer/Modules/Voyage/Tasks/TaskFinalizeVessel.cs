using AutoRetainer.Internal;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal unsafe static class TaskFinalizeVessel
{
    internal static void Enqueue(string name, VoyageType type, bool quit)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskFinalizeVessel)} name={name}, type={type}, quit={quit}");
        TaskSelectVesselByName.Enqueue(name, type);
        P.TaskManager.Enqueue(VoyageScheduler.FinalizeVessel);
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
        if(quit) P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
    }
}
