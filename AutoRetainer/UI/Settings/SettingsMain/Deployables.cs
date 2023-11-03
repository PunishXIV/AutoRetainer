using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Settings.SettingsMain
{
    internal class Deployables
    {
         static int MinLevel = 0;
        static int MaxLevel = 0;
        static string Conf = "";
        static bool InvertConf = false;
        internal static void Draw()
        {
            if (ImGuiGroup.BeginGroupBox("General"))
            {

                ImGui.Checkbox($"Resend vessels when accessing the Voyage Control Panel", ref C.SubsAutoResend);
                ImGui.Checkbox($"Finalize all vessels before resending them", ref C.FinalizeBeforeResend);
                ImGui.Checkbox($"Hide Airships from Deployables UI", ref C.HideAirships);
                ImGuiGroup.EndGroupBox();
            }
                if (ImGuiGroup.BeginGroupBox("Alert settings"))
            {
                ImGui.Checkbox($"Less than possible vessels enabled", ref C.AlertNotAllEnabled);
                ImGui.Checkbox($"Enabled vessel isn't deployed", ref C.AlertNotDeployed);
                ImGuiEx.Text($"Unoptimal submersible configuration alerts:");
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
                ImGui.SetNextItemWidth(60f);
                ImGui.DragInt("##rank1", ref MinLevel, 0.1f);
                ImGui.SameLine();
                ImGuiEx.Text($"-");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(60f);
                ImGui.DragInt("##rank2", ref MaxLevel, 0.1f);
                ImGuiEx.TextV($"Configurations:");
                ImGui.SameLine();
                ImGui.Checkbox($"NOT", ref InvertConf);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 100f);
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
                ImGuiGroup.EndGroupBox();
            }
        }
    }
}
