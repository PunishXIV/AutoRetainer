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
                    
                }
            }
            if (ImGui.CollapsingHeader("control"))
            {
                if (ImGui.Button($"{nameof(VoyageScheduler.Lockon)}")) DuoLog.Information($"{VoyageScheduler.Lockon}");
                if (ImGui.Button($"{nameof(VoyageScheduler.Approach)}")) DuoLog.Information($"{VoyageScheduler.Approach}");
                if (ImGui.Button($"{nameof(VoyageScheduler.AutomoveOff)}")) DuoLog.Information($"{VoyageScheduler.AutomoveOff}");
                if (ImGui.Button($"{nameof(VoyageScheduler.InteractWithVoyagePanel)}")) DuoLog.Information($"{VoyageScheduler.InteractWithVoyagePanel}");
                if (ImGui.Button($"{nameof(VoyageScheduler.SelectAirshipManagement)}")) DuoLog.Information($"{VoyageScheduler.SelectAirshipManagement}");
                if (ImGui.Button($"{nameof(VoyageScheduler.SelectSubManagement)}")) DuoLog.Information($"{VoyageScheduler.SelectSubManagement}");
                ImGui.InputText("subject name", ref data1, 100);
                if (ImGui.Button($"{nameof(VoyageScheduler.SelectVesselByName)}")) DuoLog.Information($"{VoyageScheduler.SelectVesselByName(data1)}");
                if (ImGui.Button($"{nameof(VoyageScheduler.RedeployVessel)}")) DuoLog.Information($"{VoyageScheduler.RedeployVessel}");
                if (ImGui.Button($"{nameof(VoyageScheduler.DeployVessel)}")) DuoLog.Information($"{VoyageScheduler.DeployVessel}");
                if (ImGui.Button($"{nameof(VoyageScheduler.Approach)}")) DuoLog.Information($"{VoyageScheduler.Approach}");
                if (ImGui.Button($"{nameof(VoyageScheduler.Approach)}")) DuoLog.Information($"{VoyageScheduler.Approach}");
                if (ImGui.Button($"{nameof(VoyageScheduler.Approach)}")) DuoLog.Information($"{VoyageScheduler.Approach}");
            }
            if(ImGui.CollapsingHeader("Test task manager"))
            {
                if(ImGui.Button("Test redeploy airship"))
                {
                    P.TaskManager.Enqueue(VoyageScheduler.Lockon);
                    P.TaskManager.Enqueue(VoyageScheduler.Approach);
                    P.TaskManager.Enqueue(VoyageScheduler.AutomoveOff);
                    P.TaskManager.Enqueue(VoyageScheduler.InteractWithVoyagePanel);
                    P.TaskManager.Enqueue(VoyageScheduler.SelectAirshipManagement);
                    P.TaskManager.Enqueue(() => VoyageScheduler.SelectVesselByName(data1));
                    P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
                    P.TaskManager.Enqueue(VoyageScheduler.DeployVessel);
                }
            }
        }
    }
}
