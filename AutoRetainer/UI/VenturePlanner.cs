using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI
{
    internal class VenturePlanner : Window
    {
        OfflineRetainerData SelectedRetainer = null;
        OfflineCharacterData SelectedCharacter = null;
        string search = "";

        public VenturePlanner() : base("Venture Planner")
        {
        }

        internal void Open(OfflineCharacterData characterData, OfflineRetainerData selectedRetainer)
        {
            this.SelectedCharacter = characterData;
            this.SelectedRetainer = selectedRetainer;
            this.IsOpen = true;
        }

        public override void Draw()
        {
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##selectRet", SelectedRetainer?.Name ?? "Select a retainer..."))
            {
                foreach(var x in P.config.OfflineData)
                {
                    foreach(var r in x.RetainerData)
                    {
                        if(ImGui.Selectable($"{x.Name}@{x.World} - {r.Name}"))
                        {
                            SelectedRetainer = r;
                            SelectedCharacter = x;
                        }
                    }
                }
                ImGui.EndCombo();
            }

            if (SelectedRetainer != null && SelectedCharacter != null)
            {
                var attachedData = Utils.GetAdditionalData(SelectedCharacter.CID, SelectedRetainer.Name);
                if(attachedData.VenturePlanner.Count > 0)
                {
                    ImGuiEx.TextWrapped(ImGuiColors.DalamudOrange, $"Venture planner is active. Operation settings are ignored for this retainer.");
                }
                ImGuiEx.SetNextItemFullWidth();
                ImGui.InputTextWithHint("##search", "Filter...", ref search, 100);
                if (ImGui.BeginChild("PlannerData"))
                {
                    if (ImGui.CollapsingHeader("Field explorations"))
                    {
                        foreach (var x in VentureUtils.GetFieldExplorations(SelectedRetainer.Job).Where(x => x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)))
                        {
                            ImGuiEx.CollectionCheckbox($"{x.GetVentureName()}", x.RowId, attachedData.VenturePlanner);
                        }
                    }
                    if (ImGui.CollapsingHeader("Hunting"))
                    {
                        foreach (var x in VentureUtils.GetHunts(SelectedRetainer.Job).Where(x => x.GetVentureName().Contains(search, StringComparison.OrdinalIgnoreCase)))
                        {
                            ImGuiEx.CollectionCheckbox($"{x.GetVentureName()}", x.RowId, attachedData.VenturePlanner);
                        }
                    }
                    ImGui.EndChild();
                }
            }

        }
    }
}
