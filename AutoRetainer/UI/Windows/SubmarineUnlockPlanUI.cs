﻿using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;

namespace AutoRetainer.UI.Windows;

internal unsafe class SubmarineUnlockPlanUI : Window
{
    internal string SelectedPlanGuid = Guid.Empty.ToString();
    internal string SelectedPlanName => VoyageUtils.GetSubmarineUnlockPlanByGuid(SelectedPlanGuid)?.Name ?? "No or unknown plan selected";
    internal SubmarineUnlockPlan SelectedPlan => VoyageUtils.GetSubmarineUnlockPlanByGuid(SelectedPlanGuid);

    public SubmarineUnlockPlanUI() : base("Submersible Voyage Unlockable Planner")
    {
        P.WindowSystem.AddWindow(this);
    }

    internal Dictionary<uint, bool> RouteUnlockedCache = [];
    internal Dictionary<uint, bool> RouteExploredCache = [];
    internal int NumUnlockedSubs = 0;

    internal bool IsMapUnlocked(uint map, bool bypassCache = false)
    {
        if(!IsSubDataAvail()) return false;
        var throttle = $"Voyage.MapUnlockedCheck.{map}";
        if(!bypassCache && RouteUnlockedCache.TryGetValue(map, out var val) && !EzThrottler.Check(throttle))
        {
            return val;
        }
        else
        {
            EzThrottler.Throttle(throttle, 2500, true);
            RouteUnlockedCache[map] = HousingManager.IsSubmarineExplorationUnlocked((byte)map);
            return RouteUnlockedCache[map];
        }
    }

    internal bool IsMapExplored(uint map, bool bypassCache = false)
    {
        if(!IsSubDataAvail()) return false;
        var throttle = $"Voyage.MapExploredCheck.{map}";
        if(!bypassCache && RouteExploredCache.TryGetValue(map, out var val) && !EzThrottler.Check(throttle))
        {
            return val;
        }
        else
        {
            EzThrottler.Throttle(throttle, 2500, true);
            RouteExploredCache[map] = HousingManager.IsSubmarineExplorationExplored((byte)map);
            return RouteExploredCache[map];
        }
    }

    internal int? GetNumUnlockedSubs()
    {
        if(!IsSubDataAvail()) return null;
        NumUnlockedSubs = 1 + Unlocks.PointToUnlockPoint.Where(x => x.Value.Sub).Where(x => IsMapExplored(x.Key)).Count();
        return NumUnlockedSubs;
    }

    internal bool IsSubDataAvail()
    {
        if(HousingManager.Instance()->WorkshopTerritory == null) return false;
        if(HousingManager.Instance()->WorkshopTerritory->Submersible.Data.Length == 0) return false;
        if(HousingManager.Instance()->WorkshopTerritory->Submersible.Data[0].Name[0] == 0) return false;
        return true;
    }

    internal int GetAmountOfOtherPlanUsers(string guid)
    {
        var i = 0;
        C.OfflineData.Where(x => x.CID != Player.CID).Each(x => i += x.AdditionalSubmarineData.Count(a => a.Value.SelectedUnlockPlan == guid));
        return i;
    }

    public override void Draw()
    {
        C.SubmarineUnlockPlans.RemoveAll(x => x.Delete);
        ImGuiEx.InputWithRightButtonsArea("SUPSelector", () =>
        {
            if(ImGui.BeginCombo("##supsel", SelectedPlanName, ImGuiComboFlags.HeightLarge))
            {
                foreach(var x in C.SubmarineUnlockPlans)
                {
                    if(ImGui.Selectable(x.Name + $"##{x.GUID}"))
                    {
                        SelectedPlanGuid = x.GUID;
                    }
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGui.Button("New plan"))
            {
                var x = new SubmarineUnlockPlan();
                x.Name = $"Plan {x.GUID}";
                C.SubmarineUnlockPlans.Add(x);
                SelectedPlanGuid = x.GUID;
            }
        });
        ImGui.Separator();
        if(SelectedPlan == null)
        {
            ImGuiEx.Text($"No or unknown plan is selected");
        }
        else
        {
            if(Data != null)
            {
                var users = GetAmountOfOtherPlanUsers(SelectedPlanGuid);
                var my = Data.AdditionalSubmarineData.Where(x => x.Value.SelectedUnlockPlan == SelectedPlanGuid);
                if(users == 0)
                {
                    if(!my.Any())
                    {
                        ImGuiEx.TextWrapped($"This plan is not used by any submersibles.");
                    }
                    else
                    {
                        ImGuiEx.TextWrapped($"This plan is used by {my.Select(X => X.Key).Print()}.");
                    }
                }
                else
                {
                    if(!my.Any())
                    {
                        ImGuiEx.TextWrapped($"This plan is used by {users} submersibles of your other characters.");
                    }
                    else
                    {
                        ImGuiEx.TextWrapped($"This plan is used by {my.Select(X => X.Key).Print()} and {users} more submersibles on other characters.");
                    }
                }
            }
            if(C.DefaultSubmarineUnlockPlan == SelectedPlanGuid)
            {
                ImGuiEx.Text($"This plan is set as default.");
                ImGui.SameLine();
                if(ImGui.SmallButton("Reset")) C.DefaultSubmarineUnlockPlan = "";
            }
            else
            {
                if(ImGui.SmallButton("Set this plan as default")) C.DefaultSubmarineUnlockPlan = SelectedPlanGuid;
            }
            ImGuiEx.TextV("Name: ");
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputText($"##planname", ref SelectedPlan.Name, 100);
            ImGuiEx.LineCentered($"planbuttons", () =>
            {
                ImGuiEx.TextV($"Apply this plan to:");
                ImGui.SameLine();
                if(ImGui.Button("ALL submersibles"))
                {
                    C.OfflineData.Each(x => x.AdditionalSubmarineData.Each(s => s.Value.SelectedUnlockPlan = SelectedPlanGuid));
                }
                ImGui.SameLine();
                if(ImGui.Button("Current character's submersibles"))
                {
                    Data.AdditionalSubmarineData.Each(s => s.Value.SelectedUnlockPlan = SelectedPlanGuid);
                }
                ImGui.SameLine();
                if(ImGui.Button("No submersibles"))
                {
                    C.OfflineData.Each(x => x.AdditionalSubmarineData.Where(s => s.Value.SelectedUnlockPlan == SelectedPlanGuid).Each(s => s.Value.SelectedUnlockPlan = Guid.Empty.ToString()));
                }
            });
            ImGuiEx.LineCentered($"planbuttons2", () =>
            {
                if(ImGui.Button($"Copy plan settings"))
                {
                    Copy(JsonConvert.SerializeObject(SelectedPlan));
                }
                ImGui.SameLine();
                if(ImGui.Button($"Paste plan settings"))
                {
                    try
                    {
                        SelectedPlan.CopyFrom(JsonConvert.DeserializeObject<SubmarineUnlockPlan>(Paste()));
                    }
                    catch(Exception ex)
                    {
                        DuoLog.Error($"Could not import plan: {ex.Message}");
                        ex.Log();
                    }
                }
                ImGui.SameLine();
                if(ImGuiEx.ButtonCtrl("Delete this plan"))
                {
                    SelectedPlan.Delete = true;
                }
                ImGui.SameLine();
                if(ImGui.Button($"Help"))
                {
                    Svc.Chat.Print($"Here is the list of all points that can be unlocked. Whenever a plugin needs to select something to unlock, a first available destination will be chosen from this list. Please note that you can NOT simply specify end point of unlocking, you need to select ALL destinations on your way.");
                }
            });
            if(ImGui.BeginChild("Plan"))
            {
                if(!IsSubDataAvail())
                {
                    ImGuiEx.TextWrapped($"Access submarine list to retrieve data.");
                }
                ImGui.Checkbox($"Unlock submarine slots. Current slots: {GetNumUnlockedSubs()?.ToString() ?? "Unknown"}/4", ref SelectedPlan.UnlockSubs);
                ImGuiEx.TextWrapped($"Unlocking slots is always prioritized over unlocking routes.");
                ImGui.Checkbox("Enforce Spam one destination mode in Deep sea site.", ref SelectedPlan.EnforceDSSSinglePoint);
                ImGui.Checkbox("Set this plan as enforced.", ref SelectedPlan.EnforcePlan);
                ImGuiEx.HelpMarker("Any point selected for unlock in this map will be executed by every single eligible submarine until everything is actually unlocked");
                if(ImGui.BeginTable("##planTable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Zone", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Map");
                    ImGui.TableSetupColumn("Unlocked by");
                    ImGui.TableHeadersRow();
                    foreach(var x in Unlocks.PointToUnlockPoint)
                    {
                        if(x.Value.Point < 9000)
                        {
                            ImGuiEx.PushID($"{x.Key}");
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            var data = Svc.Data.GetExcelSheet<SubmarineExploration>().GetRowOrDefault(x.Key);
                            if(data != null)
                            {
                                try
                                {
                                    var col = IsMapUnlocked(x.Key);
                                    ImGuiEx.CollectionCheckbox($"{data?.FancyDestination()}", x.Key, SelectedPlan.ExcludedRoutes, true);
                                    if(col) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedGreen);
                                    if(col) ImGui.PopStyleColor();
                                    ImGui.TableNextColumn();
                                    ImGuiEx.TextV($"{data?.Map.ValueNullable?.Name}");
                                    ImGui.TableNextColumn();
                                    var notEnabled = !SelectedPlan.ExcludedRoutes.Contains(x.Key) && SelectedPlan.ExcludedRoutes.Contains(x.Value.Point);
                                    ImGuiEx.TextV(notEnabled ? ImGuiColors.DalamudRed : null, $"{Svc.Data.GetExcelSheet<SubmarineExploration>().GetRowOrDefault(x.Value.Point)?.FancyDestination()}");
                                }
                                catch(Exception e)
                                {
                                    e.Log();
                                }
                            }
                            ImGui.PopID();
                        }
                    }
                    ImGui.EndTable();
                }
                if(ImGui.CollapsingHeader("Display current point exploration order"))
                {
                    ImGuiEx.Text(SelectedPlan.GetPrioritizedPointList().Select(x => $"{Svc.Data.GetExcelSheet<SubmarineExploration>().GetRow(x.point).Destination} ({x.justification})").Join("\n"));
                }
            }
            ImGui.EndChild();
        }
    }
}
