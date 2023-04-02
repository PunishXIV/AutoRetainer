using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI
{
    internal class VenturePlanner : Window
    {
        OfflineRetainerData SelectedRetainer = null;
        OfflineCharacterData SelectedCharacter = null;
        string search = "";

        public VenturePlanner() : base("Venture Planner")
        {
        }

        internal void Open(OfflineCharacterData characterData, OfflineRetainerData selectedRetainer)
        {
            this.SelectedCharacter = characterData;
            this.SelectedRetainer = selectedRetainer;
            this.IsOpen = true;
        }

        public override void Draw()
        {
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##selectRet", $"{SelectedCharacter.Name}@{SelectedCharacter.World} - {SelectedRetainer.Name} - {SelectedRetainer.Level} {ExcelJobHelper.GetJobNameById(SelectedRetainer.Job)}" ?? "Select a retainer..."))
            {
                foreach(var x in P.config.OfflineData)
                {
                    foreach(var r in x.RetainerData)
                    {
                        if(ImGui.Selectable($"{x.Name}@{x.World} - {r.Name} - Lv{r.Level} {ExcelJobHelper.GetJobNameById(r.Job)}"))
                        {
                            SelectedRetainer = r;
                            SelectedCharacter = x;
                        }
                    }
                }
                ImGui.EndCombo();
            }

            if (SelectedRetainer != null && SelectedCharacter != null)
            {
                var adata = Utils.GetAdditionalData(SelectedCharacter.CID, SelectedRetainer.Name);
                var ww = ImGui.GetContentRegionAvail().X;
                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, ww / 2);

                if(P.config.SavedPlans.Count > 0)
                {
                    if(ImGui.Checkbox("Enable planner", ref adata.EnablePlanner))
                    {
                        if (adata.EnablePlanner)
                        {
                            adata.VenturePlanIndex = 0;
                        }
                    }
                    ImGuiEx.SetNextItemFullWidth();
                    if(ImGui.BeginCombo("##load", "Load saved plan..."))
                    {
                        int? toRem = null;
                        for (int i = 0; i < P.config.SavedPlans.Count; i++)
                        {
                            var p = P.config.SavedPlans[i];
                            ImGui.PushID(p.GUID);
                            if(ImGui.Selectable(p.Name))
                            {
                                adata.VenturePlan = p.JSONClone();
                                adata.VenturePlanIndex = 0;
                            }
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup($"Context");
                            }
                            if (ImGui.BeginPopup($"Context"))
                            {
                                if(ImGui.Selectable("Delete plan"))
                                {
                                    toRem = i;
                                }
                                ImGui.EndPopup();
                            }
                            ImGui.PopID();
                        }
                        if(toRem != null)
                        {
                            P.config.SavedPlans.RemoveAt(toRem.Value);
                        }
                        ImGui.EndCombo();
                    }
                    //ImGui.Separator();
                }

                {
                    int? toRem = null;

                    for (int i = 0; i < adata.VenturePlan.List.Count; i++)
                    {
                        var v = adata.VenturePlan.List[i];
                        ImGui.PushID(v.GUID);
                        {
                            var d = i == 0;
                            if (d) ImGui.BeginDisabled();
                            if (ImGui.ArrowButton("##up", ImGuiDir.Up))
                            {
                                Safe(() => (adata.VenturePlan.List[i], adata.VenturePlan.List[i - 1]) = (adata.VenturePlan.List[i - 1], adata.VenturePlan.List[i]));
                            }
                            if (d) ImGui.EndDisabled();
                        }
                        ImGui.SameLine();
                        {
                            var d = i == adata.VenturePlan.List.Count - 1;
                            if (d) ImGui.BeginDisabled();
                            if (ImGui.ArrowButton("##down", ImGuiDir.Down))
                            {
                                Safe(() => (adata.VenturePlan.List[i], adata.VenturePlan.List[i + 1]) = (adata.VenturePlan.List[i + 1], adata.VenturePlan.List[i]));
                            }
                            if (d) ImGui.EndDisabled();
                        }
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(100f);
                        ImGui.InputInt("##cnt", ref v.Num.ValidateRange(1, 9999), 1, 1);
                        ImGui.SameLine();
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                        {
                            toRem = i;
                        }
                        ImGui.SameLine();
                        ImGuiEx.Text($"{VentureUtils.GetVentureName(v.ID)}");

                        ImGui.PopID();
                    }

                    if (toRem != null)
                    {
                        adata.VenturePlan.List.RemoveAt(toRem.Value);
                        adata.VenturePlanIndex = 0;
                    }
                }

                if(adata.VenturePlan.List.Count > 0)
                {
                    //ImGui.Separator();
                    ImGuiEx.TextV("On plan completion:");
                    ImGui.SameLine();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGuiEx.EnumCombo("##cBeh", ref adata.VenturePlan.PlanCompleteBehavior);
                    //ImGui.Separator();
                    ImGuiEx.InputWithRightButtonsArea("SavePlan", delegate
                    {
                        ImGui.InputTextWithHint("##name", "Enter plan name...", ref adata.VenturePlan.Name, 50);
                    }, delegate
                    {
                        if(ImGui.Button("Save plan"))
                        {
                            P.config.SavedPlans.Add(adata.VenturePlan.JSONClone());
                            Notify.Success($"Plan {adata.VenturePlan.Name} saved!");
                        }
                    });
                }

                ImGui.NextColumn();

                ImGuiEx.SetNextItemFullWidth();
                if(ImGui.BeginCombo("##addVenture", "Add venture...", ImGuiComboFlags.HeightLargest))
                {
                    ImGuiEx.SetNextItemFullWidth();
                    ImGui.InputTextWithHint("##search", "Filter...", ref search, 100);
                    if(ImGui.BeginChild("##ventureCh", new(ImGui.GetContentRegionAvail().X, ImGuiHelpers.MainViewport.Size.Y / 3)))
                    {
                        if (ImGui.CollapsingHeader("Field explorations"))
                        {
                            foreach (var item in VentureUtils.GetFieldExplorations(SelectedRetainer.Job, SelectedRetainer.Level).Where(x => search.IsNullOrEmpty() || x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (ImGui.Selectable(VentureUtils.GetVentureName(item), adata.VenturePlan.List.Any(x => x.ID == item.RowId), ImGuiSelectableFlags.DontClosePopups))
                                {
                                    adata.VenturePlan.List.Add(new(item));
                                    adata.VenturePlanIndex = 0;
                                }
                            }
                        }
                        if (ImGui.CollapsingHeader("Hunts"))
                        {
                            foreach (var item in VentureUtils.GetHunts(SelectedRetainer.Job, SelectedRetainer.Level).Where(x => search.IsNullOrEmpty() || x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (ImGui.Selectable(VentureUtils.GetVentureName(item), adata.VenturePlan.List.Any(x => x.ID == item.RowId), ImGuiSelectableFlags.DontClosePopups))
                                {
                                    adata.VenturePlan.List.Add(new(item));
                                    adata.VenturePlanIndex = 0;
                                }
                            }
                        }
                        ImGui.EndChild();
                    }
                    ImGui.EndCombo();
                }

                ImGui.Columns(1);
            }

        }
    }
}
