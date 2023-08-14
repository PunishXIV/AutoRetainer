using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI
{
    internal static unsafe class WorkshopUI
    {
        internal static void Draw()
        {
            foreach (var data in C.OfflineData.Where(x => x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count > 0).OrderBy(x => x.CID == Player.CID ? 1 : 0))
            {
                ImGui.PushID($"Player{data.CID}");
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
                    if (data.OfflineAirshipData.Any())
                    {
                        ImGuiGroup.BeginGroupBox("Airships");
                        foreach (var x in data.OfflineAirshipData)
                        {
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGuiEx.CollectionButtonCheckbox($"\uf13d##{x.Name}", x.Name, data.FinalizeAirships, EColor.Green);
                            ImGui.PopFont();
                            ImGui.SameLine(0, 3);
                            ImGuiEx.CollectionCheckbox($"Airship {x.Name}", x.Name, data.EnabledAirships);
                            string rightText;
                            if (x.ReturnTime != 0)
                            {
                                rightText = x.GetRemainingSeconds() > 0 ? $"Returns in {VoyageUtils.Seconds2Time(x.GetRemainingSeconds())}" : "Exploration completed";
                            }
                            else
                            {
                                rightText = $"Not occupied";
                            }
                            ImGui.SameLine();
                            ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X - 20);
                            ImGuiEx.TextV($"{rightText}");
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
                            ImGuiEx.CollectionButtonCheckbox($"\uf13d##{x.Name}", x.Name, data.FinalizeSubs, EColor.Green);
                            ImGui.PopFont();
                            ImGui.SameLine(0, 3);
                            ImGuiEx.CollectionCheckbox($"Submarine {x.Name}", x.Name, data.EnabledSubs);
                            var rightText = "";
                            if (x.ReturnTime != 0)
                            {
                                rightText = x.GetRemainingSeconds() > 0 ? $"Returns in {VoyageUtils.Seconds2Time(x.GetRemainingSeconds())}" : "Voyage completed";
                            }
                            else
                            {
                                rightText = $"Not occupied";
                            }
                            ImGui.SameLine();
                            ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X - 20);
                            ImGuiEx.TextV($"{rightText}");
                            if (x.Name != data.OfflineSubmarineData.Last().Name) ImGui.Separator();
                        }
                        ImGuiGroup.EndGroupBox();
                    }
                }
                ImGui.PopID();
            }
            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox($"Resend airships and submarines on voyage CP access", ref C.SubsAutoResend);
                ImGui.Checkbox($"Only finalize reports", ref C.SubsOnlyFinalize);
                ImGui.Checkbox($"When resending, auto-repair vessels", ref C.SubsAutoRepair);
                //ImGui.Checkbox($"On house enter task, enter workshop if possible", ref C.EnterWorkshop);
            }

            if (ImGui.CollapsingHeader("Debug"))
            {
                if (ImGui.Button("Erase offline data"))
                {
                    Utils.GetCurrentCharacterData().OfflineAirshipData.Clear();
                    Utils.GetCurrentCharacterData().OfflineSubmarineData.Clear();
                }
                if (ImGui.Button("Repair 1")) SchedulerVoyage.TryRepair(0);
                if (ImGui.Button("Repair 2")) SchedulerVoyage.TryRepair(1);
                if (ImGui.Button("Repair 3")) SchedulerVoyage.TryRepair(2);
                if (ImGui.Button("Repair 4")) SchedulerVoyage.TryRepair(3);
                if (ImGui.Button("Close repair")) SchedulerVoyage.CloseRepair();
                //if (ImGui.Button("Trigger auto repair")) TaskRepairAll.EnqueueImmediate();
                ImGui.InputText("data1", ref data1, 50);
                ImGuiEx.EnumCombo("data2", ref data2);
                if (ImGui.Button("IsVesselNeedsRepair"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.IsVesselNeedsRepair(data1, data2, out var log).Print()}\n{log.Join("\n")}");
                    }
                    catch(Exception e)
                    {
                        e.LogDuo();
                    }
                }
                if (ImGui.Button("GetSubmarineIndexByName"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.GetSubmarineIndexByName(data1)}");
                    }
                    catch (Exception e)
                    {
                        e.LogDuo();
                    }
                }
                ImGuiEx.Text($"Bell: {Utils.GetReachableRetainerBell()}");
            }
        }
        static string data1 = "";
        static VoyageType data2 = VoyageType.Airship;
    }
}
