using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugVenture : DebugUIEntry
{
    internal int VentureID = 0;
    internal string VentureName = "";
    public override void Draw()
    {
        {
            var agent = AgentModule.Instance()->GetAgentByInternalId((AgentId)140);
            if (agent != null && agent->IsAgentActive())
            {
                ImGuiEx.TextCopy($"{(nint)agent:X16}");
                ImGuiEx.Text($"{*(ushort*)((uint)agent + 456)}");
            }
        }
        if (TryGetAddonByName<AddonRetainerTaskAsk>("RetainerTaskAsk", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            ImGuiEx.Text($"Enabled: {addon->AssignButton->IsEnabled}");
        }

        foreach (var x in C.OfflineData)
        {
            foreach (var r in x.RetainerData)
            {
                var adata = Utils.GetAdditionalData(x.CID, r.Name);
                ImGuiEx.Text($"{x.Name}@{x.World} - {r.Name} last venture index: {adata.VenturePlanIndex}, next venture: {adata.GetNextPlannedVenture()}/{VentureUtils.GetVentureName(adata.GetNextPlannedVenture())}");
            }
        }
        ImGui.InputInt("Venture id", ref VentureID);
        ImGui.InputText("Venture name", ref VentureName, 100);
        //if (ImGui.Button("SearchVentureByName")) DuoLog.Information(RetainerHandlers.SearchVentureByName(VentureName).ToString());
        if (ImGui.Button("Clear Venture list")) DuoLog.Information(RetainerHandlers.ClearTaskSupplylist().ToString());
        if (ImGui.Button("SelectSpecificVenture Name")) DuoLog.Information(RetainerHandlers.SelectSpecificVentureByName(VentureName).ToString());
        if (ImGui.Button("TaskAssignHuntingVenture"))
        {
            TaskAssignHuntingVenture.Enqueue((uint)VentureID);
        }
        if (ImGui.Button("TaskAssignFieldExploration"))
        {
            TaskAssignFieldExploration.Enqueue((uint)VentureID);
        }
        if (ImGui.Button("Select"))
        {
            RetainerHandlers.SelectSpecificVenture((uint)VentureID);
        }
        if (ImGui.CollapsingHeader("Ventures"))
        {
            var data = CSFramework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.GetStringArrayData(95);
            if (data != null)
            {
                for (var i = 0; i < data->AtkArrayData.Size; i++)
                {
                    var item = data->StringArray[i];
                    if (item != null)
                    {
                        var str = MemoryHelper.ReadSeStringNullTerminated((nint)item);
                        ImGuiEx.Text($"{i}: {str.ExtractText()}");
                    }
                    else
                    {
                        ImGuiEx.Text($"{i}: null");
                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("GetAvailableVentureNames"))
        {
            foreach (var x in VentureUtils.GetAvailableVentureNames())
            {
                ImGuiEx.Text($"{x}");
            }
        }
    }
}
