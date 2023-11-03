using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Settings.SettingsMain
{
    internal class Exclusions
    {
        internal static void Draw()
        {
            C.OfflineData.RemoveAll(x => C.Blacklist.Any(z => z.CID == x.CID));
            if (ImGuiGroup.BeginGroupBox("Configure exclusions"))
            {
                foreach (var x in C.OfflineData)
                {
                    if (ImGui.BeginTable("##excl", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInnerH))
                    {
                        ImGui.TableSetupColumn("1", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("2");
                        ImGui.TableSetupColumn("3");
                        ImGui.TableSetupColumn("4");
                        ImGui.TableSetupColumn("5");
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGuiEx.TextV($"{Censor.Character(x.Name, x.World)}:");
                        ImGui.TableNextColumn();
                        if (ImGui.Checkbox("Retainers", ref x.ExcludeRetainer))
                        {
                            x.Enabled = false;
                            C.SelectedRetainers.Remove(x.CID);
                        }
                        ImGui.TableNextColumn();
                        if (ImGui.Checkbox("Deployables", ref x.ExcludeWorkshop))
                        {
                            x.WorkshopEnabled = false;
                            x.EnabledSubs.Clear();
                            x.EnabledAirships.Clear();
                        }
                        ImGui.TableNextColumn();
                        ImGui.Checkbox("Login overlay", ref x.ExcludeOverlay);
                        ImGui.TableNextColumn();
                        if (ImGuiEx.IconButton("\uf057"))
                        {
                            C.Blacklist.Add((x.CID, x.Name));
                        }
                        ImGuiEx.Tooltip($"This will delete stored character data and prevent it from being ever created again, effectively excluding it from all current and future functions.");
                        ImGui.SameLine();
                        ImGui.Dummy(new(20, 1));
                    }
                    ImGui.EndTable();
                }
                ImGuiGroup.EndGroupBox();
            }
            if (C.Blacklist.Any())
            {
                if (ImGuiGroup.BeginGroupBox("Excluded Characters"))
                {
                    for (int i = 0; i < C.Blacklist.Count; i++)
                    {
                        var d = C.Blacklist[i];
                        ImGuiEx.TextV($"{d.Name} ({d.CID:X16})");
                        ImGui.SameLine();
                        if (ImGui.Button($"Delete##bl{i}"))
                        {
                            C.Blacklist.RemoveAt(i);
                            C.SelectedRetainers.Remove(d.CID);
                            break;
                        }
                    }
                    ImGuiGroup.EndGroupBox();
                }
            }
        }
    }
}
