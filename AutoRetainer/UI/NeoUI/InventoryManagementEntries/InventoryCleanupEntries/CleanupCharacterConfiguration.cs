using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public sealed unsafe class CleanupCharacterConfiguration : InventoryManagemenrBase
{
    public override string Name { get; } = "Inventory Cleanup/Character Configuration";

    public override int DisplayPriority => -20;

    public override void Draw()
    {
        ImGuiEx.TextWrapped($"Here you can assign preconfigured inventory cleanup lists to your registered characters.");
        ImGuiEx.SetNextItemFullWidth();
        ImGuiEx.FilteringInputTextWithHint("##search", "Search...", out var filter);
        if(ImGuiEx.BeginDefaultTable(["~Character", "Plan"]))
        {
            foreach(var characterData in C.OfflineData)
            {
                if(filter != "" && !characterData.NameWithWorld.Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
                ImGuiEx.PushID(characterData.Identity);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(characterData.NameWithWorldCensored);
                ImGui.TableNextColumn();
                var plan = characterData.InventoryCleanupPlan == Guid.Empty ? null : C.AdditionalIMSettings.FirstOrDefault(p => p.GUID == characterData.InventoryCleanupPlan);
                ImGui.SetNextItemWidth(200f);
                if(ImGui.BeginCombo("##chPlan", plan?.DisplayName ?? "Default Plan", ImGuiComboFlags.HeightLarge))
                {
                    if(ImGui.Selectable("Default Plan", plan == null)) characterData.InventoryCleanupPlan = Guid.Empty;
                    ImGui.Separator();
                    foreach(var cleanupPlan in C.AdditionalIMSettings)
                    {
                        ImGuiEx.PushID(cleanupPlan.ID);
                        if(ImGui.Selectable($"{cleanupPlan.DisplayName}"))
                        {
                            characterData.InventoryCleanupPlan = cleanupPlan.GUID;
                        }
                        ImGui.PopID();
                    }
                    ImGui.EndCombo();
                }
                ImGuiEx.DragDropRepopulate("CleanupPlan", plan?.GUID ?? Guid.Empty, ref characterData.InventoryCleanupPlan);

                ImGui.PopID();
            }
            ImGui.EndTable();
        }
    }
}