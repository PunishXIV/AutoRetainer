﻿using AutoRetainerAPI.Configuration;
using ECommons.ExcelServices;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.Windows;

public sealed class VenturePlanner : Window
{
    private OfflineRetainerData SelectedRetainer = null;
    private OfflineCharacterData SelectedCharacter = null;
    private string search = "";
    private int minLevel = 1;
    private int maxLevel = Player.MaxLevel;
    private Dictionary<uint, (string l, string r, bool avail)> Cache = [];

    public VenturePlanner() : base("Venture Planner")
    {
        P.WindowSystem.AddWindow(this);
    }

    internal void Open(OfflineCharacterData characterData, OfflineRetainerData selectedRetainer)
    {
        SelectedCharacter = characterData;
        SelectedRetainer = selectedRetainer;
        IsOpen = true;
    }

    public override void Draw()
    {
        ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo("##selectRet", $"{Censor.Character(SelectedCharacter.Name, SelectedCharacter.World)} - {Censor.Retainer(SelectedRetainer.Name)} - {SelectedRetainer.Level} {ExcelJobHelper.GetJobNameById(SelectedRetainer.Job)}" ?? "Select a retainer...", ImGuiComboFlags.HeightLarge))
        {
            foreach(var x in C.OfflineData.OrderBy(x => !C.NoCurrentCharaOnTop && x.CID == Player.CID ? 0 : 1))
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

        if(SelectedRetainer != null && SelectedCharacter != null)
        {
            var adata = Utils.GetAdditionalData(SelectedCharacter.CID, SelectedRetainer.Name);
            /*ImGuiEx.TextV("Share venture plan with:");
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth();
            var n = "No shared plan";
            if (adata.LinkedVenturePlan != "")
            {
                var linkedAdata = C.AdditionalData.GetOrDefault(adata.LinkedVenturePlan);
                if (linkedAdata != null)
                {
                    var linkedOCD = Utils.GetOfflineCharacterDataFromAdditionalRetainerDataKey(adata.LinkedVenturePlan);
                    var linkedORD = Utils.GetOfflineRetainerDataFromAdditionalRetainerDataKey(adata.LinkedVenturePlan);
                    n = $"{linkedOCD.Name}@{linkedOCD.World} - {linkedORD.Name}";
                }
            }
            if (ImGui.BeginCombo("##selectLinked", n))
            {
                if(ImGui.Selectable("Remove sharing"))
                {
                    adata.LinkedVenturePlan = "";
                }
                foreach (var x in C.OfflineData.OrderBy(x => !C.NoCurrentCharaOnTop && x.CID == Player.CID ? 0 : 1))
                {
                    foreach (var r in x.RetainerData)
                    {
                        if (ImGui.Selectable($"{Censor.Character(x.Name, x.World)} - {Censor.Retainer(r.Name)} - Lv{r.Level} {ExcelJobHelper.GetJobNameById(r.Job)}"))
                        {
                            adata.LinkedVenturePlan = Utils.GetAdditionalDataKey(x.CID, r.Name);
                        }
                    }
                }
                ImGui.EndCombo();
            }*/
            if(adata.LinkedVenturePlan == "")
            {
                var ww = ImGui.GetContentRegionAvail().X;
                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, ww / 2);

                {
                    int? toRem = null;

                    for(var i = 0; i < adata.VenturePlan.List.Count; i++)
                    {
                        var v = adata.VenturePlan.List[i];
                        ImGuiEx.PushID(v.GUID);
                        {
                            var d = i == 0;
                            if(d) ImGui.BeginDisabled();
                            if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowUp))
                            {
                                Safe(() => (adata.VenturePlan.List[i], adata.VenturePlan.List[i - 1]) = (adata.VenturePlan.List[i - 1], adata.VenturePlan.List[i]));
                            }
                            if(d) ImGui.EndDisabled();
                        }
                        ImGui.SameLine();
                        {
                            var d = i == adata.VenturePlan.List.Count - 1;
                            if(d) ImGui.BeginDisabled();
                            if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowDown))
                            {
                                Safe(() => (adata.VenturePlan.List[i], adata.VenturePlan.List[i + 1]) = (adata.VenturePlan.List[i + 1], adata.VenturePlan.List[i]));
                            }
                            if(d) ImGui.EndDisabled();
                        }
                        ImGui.SameLine();
                        ImGuiEx.SetNextItemWidthScaled(100f);
                        ImGui.InputInt("##cnt", ref v.Num.ValidateRange(1, 9999), 1, 1);
                        ImGui.SameLine();
                        if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                        {
                            toRem = i;
                        }
                        ImGui.SameLine();
                        ImGuiEx.Text($"{VentureUtils.GetFancyVentureName(v.ID, SelectedCharacter, SelectedRetainer, out _)}");

                        ImGui.PopID();
                    }

                    if(toRem != null)
                    {
                        adata.VenturePlan.List.RemoveAt(toRem.Value);
                        adata.VenturePlanIndex = 0;
                    }
                }


                ImGui.NextColumn();


                if(ImGui.Checkbox("Enable planner", ref adata.EnablePlanner))
                {
                    if(adata.EnablePlanner)
                    {
                        adata.VenturePlanIndex = 0;
                    }
                }

                if(C.SavedPlans.Count > 0)
                {
                    ImGuiEx.SetNextItemFullWidth();
                    if(ImGui.BeginCombo("##load", "Load saved plan...", ImGuiComboFlags.HeightLarge))
                    {
                        int? toRem = null;
                        for(var i = 0; i < C.SavedPlans.Count; i++)
                        {
                            var p = C.SavedPlans[i];
                            ImGuiEx.PushID(p.GUID);
                            if(ImGui.Selectable(p.Name))
                            {
                                adata.VenturePlan = p.JSONClone();
                                adata.VenturePlanIndex = 0;
                            }
                            if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup($"Context");
                            }
                            if(ImGui.BeginPopup($"Context"))
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
                            C.SavedPlans.RemoveAt(toRem.Value);
                        }
                        ImGui.EndCombo();
                    }
                    //ImGui.Separator();
                }


                if(adata.VenturePlan.List.Count > 0)
                {
                    //ImGui.Separator();
                    ImGuiEx.TextV("On plan completion:");
                    ImGui.SameLine();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGuiEx.EnumCombo("##cBeh", ref adata.VenturePlan.PlanCompleteBehavior);
                    //ImGui.Separator();
                    var overwrite = C.SavedPlans.Any(x => x.Name == adata.VenturePlan.Name);
                    ImGuiEx.InputWithRightButtonsArea("SavePlan", delegate
                    {
                        if(overwrite) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
                        ImGui.InputTextWithHint("##name", "Enter plan name...", ref adata.VenturePlan.Name, 50);
                        if(overwrite) ImGui.PopStyleColor();
                    }, delegate
                    {
                        if(ImGuiEx.IconButton(FontAwesomeIcon.Save))
                        {
                            if(overwrite)
                            {
                                C.SavedPlans.RemoveAll(x => x.Name == adata.VenturePlan.Name);
                            }
                            C.SavedPlans.Add(adata.VenturePlan.JSONClone());
                            Notify.Success($"Plan {adata.VenturePlan.Name} saved!");
                        }
                        ImGuiEx.Tooltip(overwrite ? "Overwrite Existing Venture Plan" : $"Save Venture Plan");
                    });
                }

                ImGuiEx.SetNextItemFullWidth();
                if(ImGui.BeginCombo("##addVenture", "Add venture...", ImGuiComboFlags.HeightLarge))
                {
                    ImGuiEx.SetNextItemFullWidth();
                    ImGui.InputTextWithHint("##search", "Filter...", ref search, 100);
                    ImGuiEx.TextV($"Level range:");
                    ImGui.SameLine();
                    ImGuiEx.SetNextItemWidthScaled(50f);
                    ImGui.DragInt("##minL", ref minLevel, 1, 1, Player.MaxLevel);
                    ImGui.SameLine();
                    ImGuiEx.Text($"-");
                    ImGui.SameLine();
                    ImGuiEx.SetNextItemWidthScaled(50f);
                    ImGui.DragInt("##maxL", ref maxLevel, 1, 1, Player.MaxLevel);
                    ImGuiEx.TextV($"Unavailable ventures:");
                    ImGui.SameLine();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGuiEx.EnumCombo("##unavail", ref C.UnavailableVentureDisplay);
                    if(ImGui.BeginChild("##ventureCh", new(ImGui.GetContentRegionAvail().X, ImGuiHelpers.MainViewport.Size.Y / 3)))
                    {
                        if(ImGui.CollapsingHeader(VentureUtils.GetHuntingVentureName(SelectedRetainer.Job)))
                        {
                            foreach(var item in VentureUtils.GetHunts(SelectedRetainer.Job).Where(x => search.IsNullOrEmpty() || x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)).Where(x => x.RetainerLevel >= minLevel && x.RetainerLevel <= maxLevel))
                            {
                                var l = "";
                                var r = "";
                                bool Avail;
                                if(Cache.TryGetValue(item.RowId, out var result))
                                {
                                    l = result.l;
                                    r = result.r;
                                    Avail = result.avail;
                                }
                                else
                                {
                                    item.GetFancyVentureName(SelectedCharacter, SelectedRetainer, out Avail, out l, out r);
                                    Cache[item.RowId] = (l, r, Avail);
                                }
                                if(Avail || C.UnavailableVentureDisplay != UnavailableVentureDisplay.Hide)
                                {
                                    var d = !Avail && C.UnavailableVentureDisplay != UnavailableVentureDisplay.Allow_selection;
                                    if(d) ImGui.BeginDisabled();
                                    var cur = ImGui.GetCursorPos();
                                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(r).X);
                                    ImGuiEx.Text(r);
                                    ImGui.SetCursorPos(cur);
                                    if(ImGui.Selectable(l, adata.VenturePlan.List.Any(x => x.ID == item.RowId), ImGuiSelectableFlags.DontClosePopups))
                                    {
                                        adata.VenturePlan.List.Add(new(item));
                                        adata.VenturePlanIndex = 0;
                                    }
                                    if(d) ImGui.EndDisabled();
                                }
                            }
                        }
                        if(ImGui.CollapsingHeader(VentureUtils.GetFieldExVentureName(SelectedRetainer.Job)))
                        {
                            foreach(var item in VentureUtils.GetFieldExplorations(SelectedRetainer.Job).Where(x => search.IsNullOrEmpty() || x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)).Where(x => x.RetainerLevel >= minLevel && x.RetainerLevel <= maxLevel))
                            {
                                var name = item.GetFancyVentureName(SelectedCharacter, SelectedRetainer, out var Avail);
                                var d = !Avail && C.UnavailableVentureDisplay != UnavailableVentureDisplay.Allow_selection;
                                if(d) ImGui.BeginDisabled();
                                if(ImGui.Selectable(name, adata.VenturePlan.List.Any(x => x.ID == item.RowId), ImGuiSelectableFlags.DontClosePopups))
                                {
                                    adata.VenturePlan.List.Add(new(item));
                                    adata.VenturePlanIndex = 0;
                                }
                                if(d) ImGui.EndDisabled();
                            }
                        }
                        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, Vector2.Zero);
                        if(ImGui.Button($"{Lang.CharDice}    Quick Exploration", ImGuiHelpers.GetButtonSize("A") with { X = ImGui.GetContentRegionAvail().X }))
                        {
                            adata.VenturePlan.List.Add(new(VentureUtils.QuickExplorationID));
                            adata.VenturePlanIndex = 0;
                        }
                        ImGui.PopStyleVar();
                        ImGui.EndChild();
                    }
                    ImGui.EndCombo();
                }
                else
                {
                    Cache.Clear();
                }

                if(adata.EnablePlanner && adata.VenturePlan.ListUnwrapped.Count > 0)
                {
                    var pct = adata.VenturePlanIndex / (float)adata.VenturePlan.ListUnwrapped.Count;

                    if(ImGuiEx.IconButton(Lang.IconRefresh))
                    {
                        adata.VenturePlanIndex = 0;
                    }
                    ImGui.SameLine();
                    ImGuiEx.Tooltip("Cancels remaining ventures from this plan and starts from the beginning");
                    ImGui.ProgressBar(pct, new Vector2(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GetButtonSize("X").Y));
                }

                if(C.Verbose)
                {
                    if(ImGui.CollapsingHeader("Debug"))
                    {
                        ImGuiEx.InputUint("Index", ref adata.VenturePlanIndex);
                    }
                }

                ImGui.Columns(1);
            }
            else
            {
                ImGuiEx.TextWrapped($"This retainer's venture plan is shared with different retainer's venture plan.");
            }
        }

    }
}
