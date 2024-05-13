using AutoRetainer.Modules.Voyage.Readers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskDeployAndSkipCutscene
{
		internal static void Enqueue(bool validate = false)
		{
				VoyageUtils.Log($"Task enqueued: {nameof(TaskDeployAndSkipCutscene)} validate={validate}");
				if (validate) P.TaskManager.Enqueue(ValidateDeployment);
				P.TaskManager.Enqueue(VoyageScheduler.DeployVessel);
				P.TaskManager.Enqueue(VoyageScheduler.WaitForCutscene);
				P.TaskManager.Enqueue(VoyageScheduler.WaitForNoCutscene);
		}

		internal static bool? ValidateDeployment()
		{
				if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon))
				{
						var r = new ReaderAirShipExplorationDetail(addon);
						if (r.CanDeploy) return true;
				}
				return false;
		}
}
