using NightmareUI.PrimaryUI;

namespace AutoRetainer.UI.NeoUI;
public class DeployablesTab : NeoUIEntry
{
		public override string Path => "Deployables";

		private static int MinLevel = 0;
		private static int MaxLevel = 0;
		private static string Conf = "";
		private static bool InvertConf = false;

		public override NuiBuilder Builder { get; init; } = new NuiBuilder()
				.Section("General")
				.Checkbox($"Resend vessels when accessing the Voyage Control Panel", () => ref C.SubsAutoResend2)
								.Checkbox($"Finalize all vessels before resending them", () => ref C.FinalizeBeforeResend)
								.Checkbox($"Hide Airships from Deployables UI", () => ref C.HideAirships)

				.Section("Alert Settings")
				.Checkbox($"Less than possible vessels enabled", () => ref C.AlertNotAllEnabled)
								.Checkbox($"Enabled vessel isn't deployed", () => ref C.AlertNotDeployed)
				.Widget("Unoptimal submersible configuration alerts:", (z) =>
				{
						foreach (var x in C.UnoptimalVesselConfigurations)
						{
								ImGuiEx.Text($"Rank {x.MinRank}-{x.MaxRank}, {(x.ConfigurationsInvert ? "NOT " : "")} {x.Configurations.Print()}");
								if (ImGuiEx.HoveredAndClicked("Ctrl+click to delete", default, true))
								{
										var t = x.GUID;
										new TickScheduler(() => C.UnoptimalVesselConfigurations.RemoveAll(x => x.GUID == t));
								}
						}

						ImGuiEx.TextV($"Rank:");
						ImGui.SameLine();
						ImGuiEx.SetNextItemWidthScaled(60f);
						ImGui.DragInt("##rank1", ref MinLevel, 0.1f);
						ImGui.SameLine();
						ImGuiEx.Text($"-");
						ImGui.SameLine();
						ImGuiEx.SetNextItemWidthScaled(60f);
						ImGui.DragInt("##rank2", ref MaxLevel, 0.1f);
						ImGuiEx.TextV($"Configurations:");
						ImGui.SameLine();
						ImGui.Checkbox($"NOT", ref InvertConf);
						ImGui.SameLine();
						ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 100f.Scale());
						ImGui.InputText($"##conf", ref Conf, 3000);
						ImGui.SameLine();
						if (ImGui.Button("Add"))
						{
								C.UnoptimalVesselConfigurations.Add(new()
								{
										Configurations = Conf.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
										MinRank = MinLevel,
										MaxRank = MaxLevel,
										ConfigurationsInvert = InvertConf
								});
						}
				});


}
