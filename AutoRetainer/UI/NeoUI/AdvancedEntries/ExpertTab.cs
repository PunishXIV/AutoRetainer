using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class ExpertTab : NeoUIEntry
{
		public override string Path => "Advanced/Expert Settings";

		public override NuiBuilder Builder { get; init; } = new NuiBuilder()
				.Section("Behavior")
				.EnumComboFullWidth(null, "Action on accessing retainer bell if no ventures available:", () => ref C.OpenBellBehaviorNoVentures)
				.EnumComboFullWidth(null, "Action on accessing retainer bell if any ventures available:", () => ref C.OpenBellBehaviorWithVentures)
				.EnumComboFullWidth(null, "Task completion behavior after accessing bell:", () => ref C.TaskCompletedBehaviorAccess)
				.EnumComboFullWidth(null, "Task completion behavior after manual enabling:", () => ref C.TaskCompletedBehaviorManual)
				.EnumComboFullWidth(null, "Task completion behavior during plugin operation:", () => ref C.TaskCompletedBehaviorAuto)
				.TextWrapped(ImGuiColors.DalamudGrey, "\"Close retainer list and disable plugin\" option for 3 previous settings is enforced during MultiMode operation.")
				.Checkbox("Stay in retainer menu if there are retainers to finish ventures within 5 minutes or less", () => ref C.Stay5, "This option is enforced during MultiMode operation.")
				.Checkbox($"Auto-disable plugin when closing retainer list", () => ref C.AutoDisable, "Only applies when you exit menu by yourself. Otherwise, settings above apply.")
				.Checkbox($"Do not show plugin status icons", () => ref C.HideOverlayIcons)
				.Checkbox($"Display multi mode type selector", () => ref C.DisplayMMType)
				.Checkbox($"Display deployables checkbox in workshop", () => ref C.ShowDeployables)
				.Checkbox("Enable bailout module", () => ref C.EnableBailout)
				.InputInt(150f, "Timeout before AutoRetainer will attempt to unstuck, seconds", () => ref C.BailoutTimeout)

				.Section("Settings")
				.Checkbox($"Disable sorting and collapsing/expanding", () => ref C.NoCurrentCharaOnTop)
				.Checkbox($"Show MultiMode checkbox on plugin UI bar", () => ref C.MultiModeUIBar)
				.SliderIntAsFloat(100f, "Retainer menu delay, seconds", () => ref C.RetainerMenuDelay.ValidateRange(0, 2000), 0, 2000)
				.Checkbox($"Allow venture timer to display negative values", () => ref C.TimerAllowNegative)
				.Checkbox($"Do not error check venture planner", () => ref C.NoErrorCheckPlanner2)
				.Widget("Market Cooldown Overlay", (x) =>
				{
						if (ImGui.Checkbox(x, ref C.MarketCooldownOverlay))
						{
								if (C.MarketCooldownOverlay)
								{
										P.Memory.OnReceiveMarketPricePacketHook?.Enable();
								}
								else
								{
										P.Memory.OnReceiveMarketPricePacketHook?.Disable();
								}
						}
				})
				.Checkbox($"Housing Bell Support", () => ref C.ExpertMultiAllowHET)
				.Widget("", (x) => ImGuiEx.HelpMarker("A Summoning Bell must be within range of the spawn point once the home is entered, or a workshop must be purchased.", ImGuiColors.DalamudOrange, FontAwesomeIcon.ExclamationTriangle.ToIconString()))
				 .Checkbox($"Upon activating Multi Mode, attempt to enter nearby house", () => ref C.MultiHETOnEnable)

				.Section("Integrations")
				.Checkbox($"Artisan integration", () => ref C.ArtisanIntegration, "Automatically enables AutoRetainer while Artisan is Pauses Artisan operation when ventures are ready to be collected and a retainer bell is within range. Once ventures have been dealt with Artisan will be enabled and resume whatever it was doing.")

				.Section("Server Time")
				.Checkbox("Use server time instead of PC time", () => ref C.UseServerTime)

				.Section("Utility")
				.Widget("Cleanup ghost retainers", (x) =>
				{
						if (ImGui.Button(x))
						{
								var i = 0;
								foreach (var d in C.OfflineData)
								{
										i += d.RetainerData.RemoveAll(x => x.Name == "");
								}
								DuoLog.Information($"Cleaned {i} entries");
						}
				});

		public override bool ShouldDisplay()
		{
				return C.Expert;
		}
}
