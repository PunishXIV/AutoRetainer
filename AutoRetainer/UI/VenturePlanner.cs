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
        int minLevel = 1;
        int maxLevel = 90;

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
            if(ImGui.BeginCombo("##selectRet", $"{Censor.Character(SelectedCharacter.Name, SelectedCharacter.World)} - {Censor.Retainer(SelectedRetainer.Name)} - {SelectedRetainer.Level} {ExcelJobHelper.GetJobNameById(SelectedRetainer.Job)}" ?? "Select a retainer..."))
            {
                foreach(var x in P.config.OfflineData)
                {
                    foreach(var r in x.RetainerData)
                    {
                        if(ImGui.Selectable($"{Censor.Character(x.Name, x.World)} - {Censor.Retainer(r.Name)} - Lv{r.Level} {ExcelJobHelper.GetJobNameById(r.Job)}"))
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
                        ImGuiEx.Text($"{VentureUtils.GetFancyVentureName(v.ID, SelectedCharacter, SelectedRetainer, out _)}");

                        ImGui.PopID();
                    }

                    if (toRem != null)
                    {
                        adata.VenturePlan.List.RemoveAt(toRem.Value);
                        adata.VenturePlanIndex = 0;
                    }
                }


                ImGui.NextColumn();


                if (ImGui.Checkbox("Enable planner", ref adata.EnablePlanner))
                {
                    if (adata.EnablePlanner)
                    {
                        adata.VenturePlanIndex = 0;
                    }
                }

                if (P.config.SavedPlans.Count > 0)
                {
                    ImGuiEx.SetNextItemFullWidth();
                    if (ImGui.BeginCombo("##load", "Load saved plan..."))
                    {
                        int? toRem = null;
                        for (int i = 0; i < P.config.SavedPlans.Count; i++)
                        {
                            var p = P.config.SavedPlans[i];
                            ImGui.PushID(p.GUID);
                            if (ImGui.Selectable(p.Name))
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
                                if (ImGui.Selectable("Delete plan"))
                                {
                                    toRem = i;
                                }
                                ImGui.EndPopup();
                            }
                            ImGui.PopID();
                        }
                        if (toRem != null)
                        {
                            P.config.SavedPlans.RemoveAt(toRem.Value);
                        }
                        ImGui.EndCombo();
                    }
                    //ImGui.Separator();
                }


                if (adata.VenturePlan.List.Count > 0)
                {
                    //ImGui.Separator();
                    ImGuiEx.TextV("On plan completion:");
                    ImGui.SameLine();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGuiEx.EnumCombo("##cBeh", ref adata.VenturePlan.PlanCompleteBehavior);
                    //ImGui.Separator();
                    var overwrite = P.config.SavedPlans.Any(x => x.Name == adata.VenturePlan.Name);
                    ImGuiEx.InputWithRightButtonsArea("SavePlan", delegate
                    {
                        if (overwrite) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
                        ImGui.InputTextWithHint("##name", "Enter plan name...", ref adata.VenturePlan.Name, 50);
                        if (overwrite) ImGui.PopStyleColor();
                    }, delegate
                    {
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Save))
                        {
                            if (overwrite)
                            {
                                P.config.SavedPlans.RemoveAll(x => x.Name == adata.VenturePlan.Name);
                            }
                            P.config.SavedPlans.Add(adata.VenturePlan.JSONClone());
                            Notify.Success($"Plan {adata.VenturePlan.Name} saved!");
                        }
                        ImGuiEx.Tooltip(overwrite ? "Overwrite Existing Venture Plan" : $"Save Venture Plan");
                    });
                }

                ImGuiEx.SetNextItemFullWidth();
                if(ImGui.BeginCombo("##addVenture", "Add venture...", ImGuiComboFlags.HeightLargest))
                {
                    ImGuiEx.SetNextItemFullWidth();
                    ImGui.InputTextWithHint("##search", "Filter...", ref search, 100);
                    ImGuiEx.TextV($"Level range:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(50f);
                    ImGui.DragInt("##minL", ref minLevel, 1, 1, 90);
                    ImGui.SameLine();
                    ImGuiEx.Text($"-");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(50f);
                    ImGui.DragInt("##maxL", ref maxLevel, 1, 1, 90);
                    ImGuiEx.TextV($"Unavailable ventures:");
                    ImGui.SameLine();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGuiEx.EnumCombo("##unavail", ref P.config.UnavailableVentureDisplay);
                    if (ImGui.BeginChild("##ventureCh", new(ImGui.GetContentRegionAvail().X, ImGuiHelpers.MainViewport.Size.Y / 3)))
                    {
                        if (ImGui.CollapsingHeader(VentureUtils.GetHuntingVentureName(SelectedRetainer.Job)))
                        {
                            foreach (var item in VentureUtils.GetHunts(SelectedRetainer.Job).Where(x => search.IsNullOrEmpty() || x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)).Where(x => x.RetainerLevel >= minLevel && x.RetainerLevel <= maxLevel))
                            {
                                var name = item.GetFancyVentureName(SelectedCharacter, SelectedRetainer, out var Avail);
                                if (Avail || P.config.UnavailableVentureDisplay != UnavailableVentureDisplay.Hide)
                                {
                                    var d = !Avail && P.config.UnavailableVentureDisplay != UnavailableVentureDisplay.Allow_selection;
                                    if (d) ImGui.BeginDisabled();
                                    if (ImGui.Selectable(name, adata.VenturePlan.List.Any(x => x.ID == item.RowId), ImGuiSelectableFlags.DontClosePopups))
                                    {
                                        adata.VenturePlan.List.Add(new(item));
                                        adata.VenturePlanIndex = 0;
                                    }
                                    if (d) ImGui.EndDisabled();
                                }
                            }
                        }
                        if (ImGui.CollapsingHeader(VentureUtils.GetFieldExVentureName(SelectedRetainer.Job)))
                        {
                            foreach (var item in VentureUtils.GetFieldExplorations(SelectedRetainer.Job).Where(x => search.IsNullOrEmpty() || x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)).Where(x => x.RetainerLevel >= minLevel && x.RetainerLevel <= maxLevel))
                            {
                                var name = VentureUtils.GetFancyVentureName(item, SelectedCharacter, SelectedRetainer, out var Avail);
                                var d = !Avail && P.config.UnavailableVentureDisplay != UnavailableVentureDisplay.Allow_selection;
                                if (d) ImGui.BeginDisabled();
                                if (ImGui.Selectable(name, adata.VenturePlan.List.Any(x => x.ID == item.RowId), ImGuiSelectableFlags.DontClosePopups))
                                {
                                    adata.VenturePlan.List.Add(new(item));
                                    adata.VenturePlanIndex = 0;
                                }
                                if (d) ImGui.EndDisabled();
                            }
                        }
                        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, Vector2.Zero);
                        if(ImGui.Button($"{Lang.CharDice}    Quick Exploration", ImGuiHelpers.GetButtonSize("A") with { X = ImGui.GetContentRegionAvail().X}))
                        {
                            adata.VenturePlan.List.Add(new(VentureUtils.QuickExplorationID));
                            adata.VenturePlanIndex = 0;
                        }
                        ImGui.PopStyleVar();
                        ImGui.EndChild();
                    }
                    ImGui.EndCombo();
                }

                if(adata.EnablePlanner && adata.VenturePlan.ListUnwrapped.Count > 0)
                {
                    var pct = (float)(adata.VenturePlanIndex) / (float)adata.VenturePlan.ListUnwrapped.Count;

                    if (ImGuiEx.IconButton(Lang.IconRefresh))
                    {
                        adata.VenturePlanIndex = 0;
                    }
                    ImGui.SameLine();
                    ImGuiEx.Tooltip("Cancels remaining ventures from this plan and starts from the beginning");
                    ImGui.ProgressBar(pct, new Vector2(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GetButtonSize("X").Y));
                }

                if(P.config.Verbose)
                {
                    if (ImGui.CollapsingHeader("Debug"))
                    {
                        ImGuiEx.InputUint("Index", ref adata.VenturePlanIndex);
                    }
                }

                ImGui.Columns(1);
            }

        }
    }
}
