using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public class MainSettings : NeoUIEntry
{
		public override string Path => "General";

		const string delayHelp = "The lower this value is the faster plugin will use actions. When dealing with low FPS or high latency you may want to increase this value. If you want the plugin to operate faster you may decrease it.";

		public override NuiBuilder Builder { get; init; } = new NuiBuilder()
						.Section("Delays")
						.Widget(100f, "Time Desynchronization Compensation", (x) => ImGuiEx.SliderInt(x, ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0), "Additional amount of seconds that will be subtracted from venture ending time to help mitigate possible issues of time desynchronization between the game and your PC.")
						.Widget("Enable frame delay", (x) => ImGui.Checkbox(x, ref C.UseFrameDelay))
						.If(() => !C.UseFrameDelay)
						.Widget(100f, "Interaction Delay, seconds", (x) => ImGuiEx.SliderIntAsFloat(x, ref C.Delay.ValidateRange(10, 1000), 20, 1000), delayHelp)
						.Else()
						.Widget(100f, "Interaction Delay, frames", (x) => ImGuiEx.SliderInt(x, ref C.FrameDelay.ValidateRange(2, 500), 2, 12), delayHelp)
						.EndIf()
						.Widget("Extra Logging", (x) => ImGui.Checkbox(x, ref C.ExtraDebug), "This option enables excessive logging for debugging purposes. It will spam your log and cause performance issues while enabled. This option will disable itself upon plugin reload or game restart.")

								.Section("Operation")
						.Widget("Assign + Reassign", (x) =>
						{
								if (ImGui.RadioButton(x, C.EnableAssigningQuickExploration && !C._dontReassign))
								{
										C.EnableAssigningQuickExploration = true;
										C.DontReassign = false;
								}
						}, "Automatically assigns enabled retainers to a Quick Venture if they have none already in progress and reassigns current venture.")
						.Widget("Collect", (x) =>
						{
								if (ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && C._dontReassign))
								{
										C.EnableAssigningQuickExploration = false;
										C.DontReassign = true;
								}
						}, "Only collect venture rewards from the retainer, and will not reassign them.\nHold CTRL when interacting with the Summoning Bell to apply this mode temporarily.")
						.Widget("Reassign", (x) =>
						{
								if (ImGui.RadioButton("Reassign", !C.EnableAssigningQuickExploration && !C._dontReassign))
								{
										C.EnableAssigningQuickExploration = false;
										C.DontReassign = false;
								}
						}, "Only reassign ventures that retainers are undertaking.")
						.Widget("RetainerSense", (x) => ImGui.Checkbox(x, ref C.RetainerSense), "AutoRetainer will automatically enable itself when the player is within interaction range of a Summoning Bell. You must remain stationary or the activation will be cancelled.")
						.Widget(200f, "Activation Time", (x) => ImGuiEx.SliderIntAsFloat(x, ref C.RetainerSenseThreshold, 1000, 100000))

						.Section("User Interface")
						.Checkbox("Anonymise Retainers", () => ref C.NoNames, "Retainer names will be redacted from general UI elements. They will not be hidden in debug menus and plugin logs however. While this option is on, character and retainer numbers are not guaranteed to be equal in different sections of a plugin (for example, retainer 1 in retainers view is not guaranteed to be the same retainer as in statistics view).")
						.Checkbox("Display Quick Menu in Retainer UI", () => ref C.UIBar)
						//.Checkbox("Opt out of custom Dalamud theme", () => ref C.NoTheme)
						.Checkbox("Display Extended Retainer Info", () => ref C.ShowAdditionalInfo, "Displays retainer item level/gathering/perception and the name of their current venture in the main UI.")
						.Widget("Do not close AutoRetainer windows on ESC key press", (x) =>
						{
								if (ImGui.Checkbox(x, ref C.IgnoreEsc)) Utils.ResetEscIgnoreByWindows();
						});
}
