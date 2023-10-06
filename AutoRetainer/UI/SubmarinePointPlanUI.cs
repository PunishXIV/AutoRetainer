using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;
using Newtonsoft.Json;

namespace AutoRetainer.UI;

internal unsafe class SubmarinePointPlanUI : Window
{
    internal string SelectedPlanGuid = Guid.Empty.ToString();
    internal string SelectedPlanName => VoyageUtils.GetSubmarinePointPlanByGuid(SelectedPlanGuid).GetPointPlanName();
    internal SubmarinePointPlan SelectedPlan => VoyageUtils.GetSubmarinePointPlanByGuid(SelectedPlanGuid);

    public SubmarinePointPlanUI() : base("Submarine planner")
    {
    }


    internal int GetAmountOfOtherPlanUsers(string guid)
    {
        var i = 0;
        C.OfflineData.Where(x => x.CID != Player.CID).Each(x => i += x.AdditionalSubmarineData.Count(a => a.Value.SelectedPointPlan == guid));
        return i;
    }

    public override void Draw()
    {
        C.SubmarinePointPlans.RemoveAll(x => x.Delete);
        ImGuiEx.InputWithRightButtonsArea("SUPSelector", () =>
        {
            if (ImGui.BeginCombo("##supsel", SelectedPlanName))
            {
                foreach (var x in C.SubmarinePointPlans)
                {
                    if (ImGui.Selectable(x.GetPointPlanName() + $"##{x.GUID}"))
                    {
                        SelectedPlanGuid = x.GUID;
                    }
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if (ImGui.Button("New plan"))
            {
                var x = new SubmarinePointPlan();
                x.Name = $"";
                C.SubmarinePointPlans.Add(x);
                SelectedPlanGuid = x.GUID;
            }
        });
        ImGui.Separator();
        if (SelectedPlan == null)
        {
            ImGuiEx.Text($"No or unknown plan is selected");
        }
        else
        {
            if (Data != null)
            {
                var users = GetAmountOfOtherPlanUsers(SelectedPlanGuid);
                var my = Data.AdditionalSubmarineData.Where(x => x.Value.SelectedPointPlan == SelectedPlanGuid) ;
                if (users == 0)
                {
                    if (!my.Any())
                    {
                        ImGuiEx.TextWrapped($"This plan is not used by any submarines.");
                    }
                    else
                    {
                        ImGuiEx.TextWrapped($"This plan is used by {my.Select(X => X.Key).Print()}.");
                    }
                }
                else
                {
                    if (!my.Any())
                    {
                        ImGuiEx.TextWrapped($"This plan is used by {users} submarines of your other characters.");
                    }
                    else
                    {
                        ImGuiEx.TextWrapped($"This plan is used by {my.Select(X => X.Key).Print()} and {users} more submarines on other characters.");
                    }
                }
            }
            ImGuiEx.TextV("Name: ");
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputText($"##planname", ref SelectedPlan.Name, 100);
            ImGuiEx.ImGuiLineCentered($"planbuttons", () =>
            {
                ImGuiEx.TextV($"Apply this plan to:");
                ImGui.SameLine();
                if (ImGui.Button("ALL submarines"))
                {
                    C.OfflineData.Each(x => x.AdditionalSubmarineData.Each(s => s.Value.SelectedPointPlan = SelectedPlanGuid));
                }
                ImGui.SameLine();
                if (ImGui.Button("Current character's submarines"))
                {
                    Data.AdditionalSubmarineData.Each(s => s.Value.SelectedPointPlan = SelectedPlanGuid);
                }
                ImGui.SameLine();
                if (ImGui.Button("No submarines"))
                {
                    C.OfflineData.Each(x => x.AdditionalSubmarineData.Where(s => s.Value.SelectedPointPlan == SelectedPlanGuid).Each(s => s.Value.SelectedPointPlan = Guid.Empty.ToString()));
                }
            });
            ImGuiEx.ImGuiLineCentered($"planbuttons2", () =>
            {
                if (ImGui.Button($"Copy plan settings"))
                {
                    ImGui.SetClipboardText(JsonConvert.SerializeObject(SelectedPlan));
                }
                ImGui.SameLine();
                if (ImGui.Button($"Paste plan settings"))
                {
                    try
                    {
                        SelectedPlan.CopyFrom(JsonConvert.DeserializeObject<SubmarinePointPlan>(ImGui.GetClipboardText()));
                    }
                    catch (Exception ex)
                    {
                        DuoLog.Error($"Could not import plan: {ex.Message}");
                        ex.Log();
                    }
                }
                ImGui.SameLine();
                if (ImGuiEx.ButtonCtrl("Delete this plan"))
                {
                    SelectedPlan.Delete = true;
                }
            });

            ImGuiEx.EzTableColumns("SubPlan", new System.Action[] 
            { 
                delegate 
                {
                    if(ImGui.BeginChild("col1"))
                    {
                        foreach(var x in Svc.Data.GetExcelSheet<SubmarineExplorationPretty>())
                        {
                            if(x.Destination.ExtractText() == "")
                            {
                                if(x.Map.Value.Name.ExtractText() != "")
                                {
                                    ImGui.Separator();
                                    ImGuiEx.Text($"{x.Map.Value.Name}:");
                                }
                                continue;
                            }
                            var disabled = !SelectedPlan.GetMapId().EqualsAny(0u, x.Map.Row) || (SelectedPlan.Points.Count >= 5 && !SelectedPlan.Points.Contains(x.RowId));
                            if (disabled) ImGui.BeginDisabled();
                            var cont = SelectedPlan.Points.Contains(x.RowId);
                            if (ImGui.Selectable(x.FancyDestination(), cont))
                            {
                                SelectedPlan.Points.Toggle(x.RowId);
                            }
                            if (disabled) ImGui.EndDisabled();
                        }
                        ImGui.EndChild();
                    }
                }, delegate
                {
                    if(ImGui.BeginChild("Col2")){
                        var map = SelectedPlan.GetMap();
                        if(map != null)
                        {
                            ImGuiEx.Text($"{map.Name}:");
                        }
                        var toRem = -1;
                        for (int i = 0; i < SelectedPlan.Points.Count; i++)
                        {
                            ImGui.PushID(i);
                            if(ImGui.ArrowButton($"##up", ImGuiDir.Up) && i > 0)
                            {
                                (SelectedPlan.Points[i-1], SelectedPlan.Points[i]) = (SelectedPlan.Points[i], SelectedPlan.Points[i-1]);
                            }
                            ImGui.SameLine();
                            if(ImGui.ArrowButton($"##down", ImGuiDir.Down) && i < SelectedPlan.Points.Count - 1)
                            {
                                (SelectedPlan.Points[i+1], SelectedPlan.Points[i]) = (SelectedPlan.Points[i], SelectedPlan.Points[i+1]);
                            }
                            ImGui.SameLine();
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                            {
                                toRem = i;
                            }
                            ImGui.SameLine();
                            ImGuiEx.Text($"{VoyageUtils.GetSubmarineExploration(SelectedPlan.Points[i]).FancyDestination()}");
                            ImGui.PopID();
                        }
                        if(toRem > -1)
                        {
                            SelectedPlan.Points.RemoveAt(toRem);
                        }
                    ImGui.EndChild();
                    }
                }
            });
        }
    }
}
