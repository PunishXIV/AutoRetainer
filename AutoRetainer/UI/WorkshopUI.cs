using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using Dalamud.Utility;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.UI
{
    internal static unsafe class WorkshopUI
    {
        internal static void Draw()
        {
            var sortedData = new List<OfflineCharacterData>();
            if (C.NoCurrentCharaOnTop)
            {
                sortedData = C.OfflineData;
            }
            else
            {
                if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var cdata))
                {
                    sortedData.Add(cdata);
                }
                foreach (var x in C.OfflineData)
                {
                    if (x.CID != Svc.ClientState.LocalContentId)
                    {
                        sortedData.Add(x);
                    }
                }
            }
            foreach (var data in sortedData.Where(x => x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count > 0))
            {
                ImGui.PushID($"Player{data.CID}");
                var rCurPos = ImGui.GetCursorPos();
                float pad = 0;
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.ButtonCheckbox($"\uf21a##{data.CID}", ref data.WorkshopEnabled, EColor.Green);
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Enable submersibles in multi mode on this character");
                ImGui.SameLine(0,3);
                if (ImGuiEx.IconButton(FontAwesomeIcon.DoorOpen))
                {
                    if (MultiMode.Active)
                    {
                        foreach (var z in C.OfflineData)
                        {
                            z.Preferred = false;
                        }
                        Notify.Warning("Preferred character has been reset");
                    }
                    if (MultiMode.Relog(data, out var error))
                    {
                        Notify.Success("Relogging...");
                    }
                    else
                    {
                        Notify.Error(error);
                    }
                }
                ImGui.SameLine(0, 3);
                if (ImGuiEx.CollapsingHeader(Censor.Character(data.Name, data.World), data.AnyEnabledVesselsAvailable()?ImGuiColors.ParsedGreen:null))
                {
                    pad = ImGui.GetStyle().FramePadding.Y;
                    if (data.OfflineAirshipData.Any())
                    {
                        ImGuiGroup.BeginGroupBox("Airships");
                        foreach (var x in data.OfflineAirshipData)
                        {
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGuiEx.CollectionButtonCheckbox($"\uf13d##{x.Name}", x.Name, data.FinalizeAirships, EColor.BlueSea);
                            ImGui.PopFont();
                            ImGuiEx.Tooltip("Enable this to only finalize this vessel");
                            ImGui.SameLine(0, 3);
                            ImGuiEx.CollectionCheckbox($"{x.Name}##airsh", x.Name, data.EnabledAirships);
                            string rightTextV;
                            if (x.ReturnTime != 0)
                            {
                                rightTextV = x.GetRemainingSeconds() > 0 ? $"{VoyageUtils.Seconds2Time(x.GetRemainingSeconds())}" : "Exploration completed";
                            }
                            else
                            {
                                rightTextV = $"Not occupied";
                            }
                            ImGui.SameLine();
                            ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightTextV).X - 20);
                            ImGuiEx.TextV($"{rightTextV}");
                            if(x.Name != data.OfflineAirshipData.Last().Name) ImGui.Separator();
                        }
                        ImGuiGroup.EndGroupBox();
                    }
                    if (data.OfflineSubmarineData.Any())
                    {
                        ImGuiGroup.BeginGroupBox("Submarines");
                        foreach (var x in data.OfflineSubmarineData)
                        {
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGuiEx.CollectionButtonCheckbox($"\uf13d##{x.Name}", x.Name, data.FinalizeSubs, EColor.BlueSea);
                            ImGui.PopFont();
                            ImGuiEx.Tooltip("Enable this to only finalize this vessel");
                            ImGui.SameLine(0, 3);
                            ImGuiEx.CollectionCheckbox($"{x.Name}##sub", x.Name, data.EnabledSubs);
                            var rightTextV = "";
                            if (x.ReturnTime != 0)
                            {
                                rightTextV = x.GetRemainingSeconds() > 0 ? $"{VoyageUtils.Seconds2Time(x.GetRemainingSeconds())}" : "Voyage completed";
                            }
                            else
                            {
                                rightTextV = $"Not occupied";
                            }
                            ImGui.SameLine();
                            ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightTextV).X - 20);
                            ImGuiEx.TextV($"{rightTextV}");
                            if (x.Name != data.OfflineSubmarineData.Last().Name) ImGui.Separator();
                        }
                        ImGuiGroup.EndGroupBox();
                    }
                }

                var rightText = $"R: {data.RepairKits} | C: {data.Ceruleum} | I: {data.InventorySpace}";
                var cur = ImGui.GetCursorPos();
                ImGui.SameLine();
                ImGui.SetCursorPos(new(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X - ImGui.GetStyle().FramePadding.X, rCurPos.Y + pad));
                ImGuiEx.Text(rightText);

                ImGui.PopID();
            }
            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox($"Resend airships and submarines on voyage CP access", ref C.SubsAutoResend);
                ImGui.Checkbox($"Only finalize reports", ref C.SubsOnlyFinalize);
                ImGui.Checkbox($"When resending, auto-repair vessels", ref C.SubsAutoRepair);
                ImGui.Checkbox($"Even when only finalizing, repair vessels", ref C.SubsRepairFinalize);
            }

            if (ImGui.CollapsingHeader("Public debug"))
            {
                try
                {
                    if (!P.TaskManager.IsBusy)
                    {
                        if (ImGui.Button("Resend currently selected submarine on previous voyage"))
                        {
                            TaskDeployOnPreviousVoyage.Enqueue();
                        }
                        foreach (var x in Data.OfflineSubmarineData)
                        {
                            if (ImGui.Button($"Repair {x.Name} submarine's broken components"))
                            {
                                if (VoyageUtils.GetCurrentWorkshopPanelType() == PanelType.Submersible)
                                {
                                    TaskSelectVesselByName.Enqueue(x.Name);
                                    TaskIntelligentRepair.Enqueue(x.Name, VoyageType.Submersible);
                                    P.TaskManager.Enqueue(VoyageScheduler.SelectVesselQuit);
                                }
                                else
                                {
                                    Notify.Error("You are not in a submersible menu");
                                }
                            }
                        }
                        if (ImGui.Button("Approach bell"))
                        {
                            TaskInteractWithNearestBell.Enqueue(false);
                        }

                        if (ImGui.Button("Approach panel"))
                        {
                            TaskInteractWithNearestPanel.Enqueue(false);
                        }

                    }
                    else
                    {
                        ImGuiEx.Text(EColor.RedBright, $"Currently executing: {P.TaskManager.CurrentTaskName}");
                    }
                }
                catch(Exception e)
                {
                    ImGuiEx.TextWrapped(e.ToString());
                }
            }
            if (ImGui.CollapsingHeader("Debug"))
            {
                if (ImGui.Button("Erase offline data"))
                {
                    Utils.GetCurrentCharacterData().OfflineAirshipData.Clear();
                    Utils.GetCurrentCharacterData().OfflineSubmarineData.Clear();
                }
                if (ImGui.Button("Repair 1")) VoyageScheduler.TryRepair(0);
                if (ImGui.Button("Repair 2")) VoyageScheduler.TryRepair(1);
                if (ImGui.Button("Repair 3")) VoyageScheduler.TryRepair(2);
                if (ImGui.Button("Repair 4")) VoyageScheduler.TryRepair(3);
                if (ImGui.Button("Close repair")) VoyageScheduler.CloseRepair();
                //if (ImGui.Button("Trigger auto repair")) TaskRepairAll.EnqueueImmediate();
                ImGui.InputText("data1", ref data1, 50);
                ImGuiEx.EnumCombo("data2", ref data2);
                if (ImGui.Button("IsVesselNeedsRepair"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.GetIsVesselNeedsRepair(data1, data2, out var log).Print()}\n{log.Join("\n")}");
                    }
                    catch (Exception e)
                    {
                        e.LogDuo();
                    }
                }
                if (ImGui.Button("GetSubmarineIndexByName"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.GetVesselIndexByName(data1, VoyageType.Submersible)}");
                    }
                    catch (Exception e)
                    {
                        e.LogDuo();
                    }
                }
                ImGuiEx.Text($"Bell: {Utils.GetReachableRetainerBell(false)}");
                ImGuiEx.Text($"Bell(true): {Utils.GetReachableRetainerBell(true)}");
                ImGuiEx.TextWrapped($"Enabled subs: {VoyageUtils.GetVesselData(Data, VoyageType.Submersible).Select(x => $"{x.Name}, {x.GetRemainingSeconds()}").Print()}");
                ImGuiEx.Text($"AnyEnabledVesselsAvailable: {VoyageUtils.AnyEnabledVesselsAvailable(Data)}");
                ImGuiEx.Text($"Panel type: {VoyageUtils.GetCurrentWorkshopPanelType()}");
                if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
                {
                    var button = addon->UldManager.NodeList[3]->GetAsAtkComponentButton();
                    ImGuiEx.Text($"Button: {button->IsEnabled}");
                }
                if(ImGui.Button("Interact with nearest panel"))
                {
                    TaskInteractWithNearestPanel.Enqueue();
                }
            }
        }
        static string data1 = "";
        static VoyageType data2 = VoyageType.Airship;


        static void DrawTable(OfflineCharacterData data)
        {
            if (ImGui.BeginTable("##retainertable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Structure");
                ImGui.TableSetupColumn("Voyage");
                ImGui.TableSetupColumn("");
                ImGui.TableHeadersRow();
                var retainers = P.GetSelectedRetainers(data.CID);
                for (var i = 0; i < data.OfflineSubmarineData.Count; i++)
                {
                    var vessel = data.OfflineSubmarineData[i];
                    if (ret.Level == 0 || ret.Name.ToString().IsNullOrEmpty()) continue;
                    var adata = Utils.GetAdditionalData(data.CID, ret.Name);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                    var start = ImGui.GetCursorPos();
                    var selected = retainers.Contains(ret.Name.ToString());
                    if (ImGui.Checkbox($"{Censor.Retainer(ret.Name)}", ref selected))
                    {
                        if (selected)
                        {
                            retainers.Add(ret.Name.ToString());
                        }
                        else
                        {
                            retainers.Remove(ret.Name.ToString());
                        }
                    }
                    if (adata.EntrustDuplicates)
                    {
                        ImGui.SameLine();
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGuiEx.Text(Lang.IconDuplicate);
                        ImGui.PopFont();
                    }
                    if (adata.WithdrawGil)
                    {
                        ImGui.SameLine();
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGuiEx.Text(Lang.IconGil);
                        ImGui.PopFont();
                    }
                    Svc.PluginInterface.GetIpcProvider<ulong, string, object>(ApiConsts.OnRetainerPostVentureTaskDraw).SendMessage(data.CID, ret.Name);
                    if (adata.IsVenturePlannerActive())
                    {
                        ImGui.SameLine();
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGuiEx.Text(Lang.IconPlanner);
                        ImGui.PopFont();
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            for (int j = 0; j < adata.VenturePlan.ListUnwrapped.Count; j++)
                            {
                                var v = adata.VenturePlan.ListUnwrapped[j];
                                if (j == adata.VenturePlanIndex - 1)
                                {
                                    ImGuiEx.Text(ImGuiColors.ParsedGreen, $"{VentureUtils.GetFancyVentureName(v, data, ret, out _)}");
                                }
                                else if (j == adata.VenturePlanIndex || (j == 0 && adata.VenturePlan.PlanCompleteBehavior == PlanCompleteBehavior.Restart_plan && adata.VenturePlanIndex >= adata.VenturePlan.ListUnwrapped.Count))
                                {
                                    ImGuiEx.Text(ImGuiColors.DalamudYellow, $"{VentureUtils.GetFancyVentureName(v, data, ret, out _)}");
                                }
                                else
                                {
                                    ImGuiEx.Text($"{VentureUtils.GetFancyVentureName(v, data, ret, out _)}");
                                }
                            }
                            ImGui.EndTooltip();
                        }
                    }
                    var end = ImGui.GetCursorPos();
                    bars[$"{data.CID}{retainerData[i].Name}"] = (start, end);
                    ImGui.TableNextColumn();
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);

                    if (ThreadLoadImageHandler.TryGetIconTextureWrap(ret.Job == 0 ? 62143 : (062100 + ret.Job), true, out var t))
                    {
                        ImGui.Image(t.ImGuiHandle, new(24, 24));
                    }
                    else
                    {
                        ImGui.Dummy(new(24, 24));
                    }
                    if (ret.Level > 0)
                    {
                        ImGui.SameLine(0, 2);
                        var level = $"{Lang.CharLevel}{ret.Level}";
                        var add = "";
                        if (adata.Ilvl > -1 && !VentureUtils.IsDoL(ret.Job))
                        {
                            add += $"{Lang.CharItemLevel}{adata.Ilvl}";
                        }
                        if ((adata.Gathering > -1 || adata.Perception > -1) && VentureUtils.IsDoL(ret.Job))
                        {
                            add += $"{Lang.CharPlant}{adata.Gathering}/{adata.Perception}";
                        }
                        bool cap = ret.Level < 90 && data.GetJobLevel(ret.Job) == ret.Level;
                        if (cap) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                        ImGuiEx.TextV(level.ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont));
                        if (cap) ImGui.PopStyleColor();
                        if (C.ShowAdditionalInfo && add != "")
                        {
                            ImGui.SameLine();
                            ImGuiEx.Text(add);
                        }
                    }
                    ImGui.TableNextColumn();
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                    if (ret.VentureID != 0 && C.ShowAdditionalInfo)
                    {
                        var parts = VentureUtils.GetVentureById(ret.VentureID).GetFancyVentureNameParts(data, ret, out _);
                        if (!parts.Name.IsNullOrEmpty())
                        {
                            var c = parts.YieldRate == 4 ? ImGuiColors.ParsedGreen : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                            ImGuiEx.Text(c, $"{(parts.Level != 0 ? $"{Lang.CharLevel}{parts.Level} " : "")}{parts.Name}");
                            ImGui.SameLine();
                        }
                    }
                    ImGuiEx.Text($"{(!ret.HasVenture ? "No Venture" : Utils.ToTimeString(ret.GetVentureSecondsRemaining(C.TimerAllowNegative)))}");
                    ImGui.TableNextColumn();
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
                    var n = $"{data.CID} {ret.Name} settings";
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Cogs, $"{data.CID} {ret.Name}"))
                    {
                        ImGui.OpenPopup(n);
                    }
                    if (ImGuiEx.BeginPopupNextToElement(n))
                    {
                        ImGui.CollapsingHeader($"{Censor.Retainer(ret.Name)} - {Censor.Character(data.Name)} Configuration  ##conf", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
                        ImGuiEx.Text($"Additional Post-venture Tasks:");
                        ImGui.Checkbox($"Entrust Duplicates", ref adata.EntrustDuplicates);
                        ImGui.Checkbox($"Withdraw/Deposit Gil", ref adata.WithdrawGil);
                        if (adata.WithdrawGil)
                        {
                            if (ImGui.RadioButton("Withdraw", !adata.Deposit)) adata.Deposit = false;
                            if (ImGui.RadioButton("Deposit", adata.Deposit)) adata.Deposit = true;
                            ImGui.SetNextItemWidth(200f);
                            ImGui.InputInt($"Amount, %", ref adata.WithdrawGilPercent.ValidateRange(1, 100), 1, 10);
                        }
                        ImGui.Separator();
                        Svc.PluginInterface.GetIpcProvider<ulong, string, object>(ApiConsts.OnRetainerSettingsDraw).SendMessage(data.CID, ret.Name);
                        ImGui.EndPopup();
                    }
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(Lang.IconPlanner, $"{data.CID} {ret.Name} planner"))
                    {
                        P.VenturePlanner.Open(data, ret);
                    }
                }
                ImGui.EndTable();
            }
            ImGui.Dummy(new(2, 2));
        }
    }
}
