using AutoRetainer.UI.Settings.SettingsMain;
using AutoRetainerAPI.Configuration;
using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeContingency : NeoUIEntry
{
		static readonly FrozenDictionary<WorkshopFailAction, string> WorkshopFailActionNames = new Dictionary<WorkshopFailAction, string>()
		{
				[WorkshopFailAction.StopPlugin] = "Halt all plugin operation",
				[WorkshopFailAction.ExcludeVessel] = "Exclude deployable from operation",
				[WorkshopFailAction.ExcludeChar] = "Exclude captain from multi mode rotation",
		}.ToFrozenDictionary();

		public override string Path => "Contingency";

		public override NuiBuilder Builder { get; init; } = new NuiBuilder()
				.Section("Contingency")
				.TextWrapped("Here you can apply various fallback actions to perform in the case of some common failure states or potential operation errors.")
				.EnumComboFullWidth(null, "Ceruleum Tanks Expended", () => ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of insufficient Ceruleum Tanks to deploy vessel on a new voyage.")
				.EnumComboFullWidth(null, "Unable to Repair Deployable", () => ref C.FailureNoRepair, null, WorkshopFailActionNames, "Applies selected fallback action in the case of insufficient Magitek Repair Materials to repair a vessel.")
				.EnumComboFullWidth(null, "Inventory at Capacity", () => ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of the captain's inventory having insufficient space to receive voyage rewards.")
				.EnumComboFullWidth(null, "Critical Operation Failure", () => ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of any unknown or miscellaneous error.")
				.Widget("Jailed by the GM", (x) =>
				{
						ImGui.BeginDisabled();
						ImGuiEx.SetNextItemFullWidth();
						if (ImGui.BeginCombo("##jailsel", "Terminate the game")) { ImGui.EndCombo(); }
						ImGui.EndDisabled();
				}, "Applies selected fallback action in the case if you got jailed by the GM while plugin is running. Good luck!");
}
