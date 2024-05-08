using AutoRetainer.UI.Settings.SettingsMain;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public class MiscTab : NeoUIEntry
{
		public override string Path => "Miscellaneous";

		public override NuiBuilder Builder { get; init; } = new NuiBuilder()
				.Section("Quick Retainer Action")
				.Widget("Sell Item", (x) => SettingsMain.QRA(x, ref C.SellKey))
				.Widget("Entrust Item", (x) => SettingsMain.QRA(x, ref C.EntrustKey))
				.Widget("Retrieve Item", (x) => SettingsMain.QRA(x, ref C.RetrieveKey))
				.Widget("Put up For Sale", (x) => SettingsMain.QRA(x, ref C.SellMarketKey))

				.Section("Statistics")
				.Checkbox($"Record Venture Statistics", () => ref C.RecordStats)

				.Section("Automatic Grand Company Expert Delivery")
				.Checkbox("Tray notification upon handin completion (requires NotificationMaster)", () => ref C.GCHandinNotify)

				.Section("Performance")

				.If(() => Utils.IsBusy)
				.Widget("", (x) => ImGui.BeginDisabled())
				.EndIf()

				.Checkbox($"Remove minimized FPS restrictions while plugin is operating", () => ref C.UnlockFPS)
				.Checkbox($"- Also remove general FPS restriction", () => ref C.UnlockFPSUnlimited)
				.Checkbox($"- Also pause ChillFrames plugin", () => ref C.UnlockFPSChillFrames)
				.Checkbox($"Raise FFXIV process priority while plugin is operating", () => ref C.ManipulatePriority, "May result other programs slowdown")

				.If(() => Utils.IsBusy)
				.Widget("", (x) => ImGui.EndDisabled())
				.EndIf();
}
