using AutoRetainer.Internal;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskRedeployVessel
{
		internal static void Enqueue(string name, VoyageType type)
		{
				VoyageUtils.Log($"Task enqueued: {nameof(TaskRedeployVessel)} name={name}, type={type}");
				P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
				TaskIntelligentRepair.Enqueue(name, type);
				TaskRedeployPreviousLog.Enqueue(name, type);
		}
}
