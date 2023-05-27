using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Memory;

namespace AutoRetainer.UI.Dbg;

internal static unsafe class DebugVenture
{
    internal static int VentureID = 0;
    internal static string VentureName = "";
    internal static void Draw()
    {
        foreach(var x in P.config.OfflineData)
        {
            foreach(var r in x.RetainerData)
            {
                var adata = Utils.GetAdditionalData(x.CID, r.Name);
                ImGuiEx.Text($"{x.Name}@{x.World} - {r.Name} last venture index: {adata.VenturePlanIndex}, next venture: {adata.GetNextPlannedVenture()}/{VentureUtils.GetVentureName(adata.GetNextPlannedVenture())}");
            }
        }
        ImGui.InputInt("Venture id", ref VentureID);
        ImGui.InputText("Venture name", ref VentureName, 100);
        if (ImGui.Button("SearchVentureByName")) DuoLog.Information(RetainerHandlers.SearchVentureByName(VentureName).ToString());
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
            if(data != null)
            {
                for (int i = 0; i < data->AtkArrayData.Size; i++)
                {
                    var item = data->StringArray[i];
                    if(item != null)
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
            foreach(var x in VentureUtils.GetAvailableVentureNames())
            {
                ImGuiEx.Text($"{x}");
            }
        }
    }
}
