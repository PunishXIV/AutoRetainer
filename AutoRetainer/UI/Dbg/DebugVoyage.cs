using AutoRetainer.Modules.Voyage;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Dbg
{
    internal static unsafe class DebugVoyage
    {
        static string data1 = "";
        static int r1, r2, r3, r4, r5 = -1;
        internal static void Draw()
        {
            if (ImGui.CollapsingHeader("data"))
            {
                ImGuiEx.Text($"HID: {HousingManager.Instance()->GetCurrentHouseId()}");
                if (HousingManager.Instance()->WorkshopTerritory != null)
                {
                    ImGuiEx.Text($"Num air: {HousingManager.Instance()->WorkshopTerritory->Airship.AirshipCount}");
                    //ImGuiEx.Text($"Num w: {HousingManager.Instance()->WorkshopTerritory->Submersible.DataList}");
                    {
                        var data = HousingManager.Instance()->WorkshopTerritory->Airship.DataListSpan;
                        for (int i = 0; i < data.Length; i++)
                        {
                            var d = data[i];
                            ImGuiEx.Text($"Air: {MemoryHelper.ReadSeStringNullTerminated((nint)d.Name).ExtractText()}, returns at {d.GetReturnTime()}, current: {d.CurrentExp}");
                        }
                    }
                    {
                        var data = HousingManager.Instance()->WorkshopTerritory->Submersible.DataListSpan;
                        for (int i = 0; i < data.Length; i++)
                        {
                            var d = data[i];
                            ImGuiEx.Text($"Sub: {MemoryHelper.ReadSeStringNullTerminated((nint)d.Name).ExtractText()}, returns at {d.GetReturnTime()}, current: {d.CurrentExp}");
                        }
                    }
                }
            }
            if (ImGui.CollapsingHeader("utils"))
            {
                ImGui.InputInt("r1", ref r1);
                if (ImGui.Button("Pick"))
                {
                    P.Memory.Use(r1);
                }
            }
            if (ImGui.CollapsingHeader("control"))
            {
                if (ImGui.Button($"{nameof(SchedulerVoyage.Lockon)}")) DuoLog.Information($"{SchedulerVoyage.Lockon}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.Approach)}")) DuoLog.Information($"{SchedulerVoyage.Approach}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.AutomoveOff)}")) DuoLog.Information($"{SchedulerVoyage.AutomoveOff}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.InteractWithVoyagePanel)}")) DuoLog.Information($"{SchedulerVoyage.InteractWithVoyagePanel}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.SelectAirshipManagement)}")) DuoLog.Information($"{SchedulerVoyage.SelectAirshipManagement}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.SelectSubManagement)}")) DuoLog.Information($"{SchedulerVoyage.SelectSubManagement}");
                ImGui.InputText("subject name", ref data1, 100);
                if (ImGui.Button($"{nameof(SchedulerVoyage.SelectVesselByName)}")) DuoLog.Information($"{SchedulerVoyage.SelectVesselByName(data1)}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.RedeployVessel)}")) DuoLog.Information($"{SchedulerVoyage.RedeployVessel}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.DeployVessel)}")) DuoLog.Information($"{SchedulerVoyage.DeployVessel}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.Approach)}")) DuoLog.Information($"{SchedulerVoyage.Approach}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.Approach)}")) DuoLog.Information($"{SchedulerVoyage.Approach}");
                if (ImGui.Button($"{nameof(SchedulerVoyage.Approach)}")) DuoLog.Information($"{SchedulerVoyage.Approach}");
            }
            if(ImGui.CollapsingHeader("Test task manager"))
            {
                if(ImGui.Button("Test redeploy airship"))
                {
                    P.TaskManager.Enqueue(SchedulerVoyage.Lockon);
                    P.TaskManager.Enqueue(SchedulerVoyage.Approach);
                    P.TaskManager.Enqueue(SchedulerVoyage.AutomoveOff);
                    P.TaskManager.Enqueue(SchedulerVoyage.InteractWithVoyagePanel);
                    P.TaskManager.Enqueue(SchedulerVoyage.SelectAirshipManagement);
                    P.TaskManager.Enqueue(() => SchedulerVoyage.SelectVesselByName(data1));
                    P.TaskManager.Enqueue(SchedulerVoyage.RedeployVessel);
                    P.TaskManager.Enqueue(SchedulerVoyage.DeployVessel);
                }
            }
        }
    }
}
