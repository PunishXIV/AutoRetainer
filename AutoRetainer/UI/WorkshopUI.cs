using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;
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
                if (ImGui.CollapsingHeader(Censor.Character(data.Name, data.World)))
                {
                    foreach (var x in data.OfflineAirshipData)
                    {
                        ImGuiEx.CollectionCheckbox($"Airship {x.Name}", x.Name, data.EnabledAirships);
                        string rightText;
                        if (x.ReturnTime != 0)
                        {
                            rightText = x.GetRemainingSeconds() > 0 ? $"returns in {x.GetRemainingSeconds()} seconds" : "Exploration completed";
                        }
                        else
                        {
                            rightText = $"Not occupied";
                        }
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X);
                        ImGuiEx.TextV($"{rightText}");
                        ImGui.Separator();
                    }
                    foreach (var x in data.OfflineSubmarineData)
                    {
                        ImGuiEx.CollectionCheckbox($"Submarine {x.Name}", x.Name, data.EnabledSubs);
                        var rightText = "";
                        if (x.ReturnTime != 0)
                        {
                            rightText = x.GetRemainingSeconds() > 0 ? $"returns in {x.GetRemainingSeconds()} seconds" : "Voyage completed";
                        }
                        else
                        {
                            rightText = $"Not occupied";
                        }
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(rightText).X);
                        ImGuiEx.TextV($"{rightText}");
                        ImGui.Separator();
                    }
                }
                ImGui.PopID();
            }
            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox($"Resend airships and submarines on voyage CP access", ref C.SubsAutoResend);
                ImGui.Checkbox($"Only finalize reports", ref C.SubsOnlyFinalize);
                ImGui.Checkbox($"When resending, auto-repair vessels", ref C.SubsAutoRepair);
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
                if (ImGui.Button("Trigger auto repair")) TaskRepairAll.EnqueueImmediate();
                ImGui.InputText("data1", ref data1, 50);
                ImGuiEx.EnumCombo("data2", ref data2);
                if (ImGui.Button("IsVesselNeedsRepair"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.IsVesselNeedsRepair(data1, data2)}");
                    }
                    catch(Exception e)
                    {
                        e.LogDuo();
                    }
                }
            }
        }
        static string data1 = "";
        static VoyageType data2 = VoyageType.Airship;
    }
}
