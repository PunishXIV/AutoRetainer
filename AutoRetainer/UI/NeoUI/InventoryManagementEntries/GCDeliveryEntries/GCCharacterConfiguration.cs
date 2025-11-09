using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GCCharacterConfiguration : InventoryManagementBase
{
    public override string Name { get; } = "Grand Company Delivery/Character Configuration";

    public override int DisplayPriority => -10;

    public override void Draw()
    {
        ImGuiEx.TextWrapped($"Here you can assign preconfigured exchange lists to your registered characters, as well as select delivery mode.");
        ImGuiEx.SetNextItemFullWidth();
        ImGuiEx.FilteringInputTextWithHint("##search", "Search...", out var filter);
        if(ImGuiEx.BeginDefaultTable(["~Character", "Plan", "Delivery mode"]))
        {
            foreach(var characterData in C.OfflineData)
            {
                if(filter != "" && !characterData.NameWithWorld.Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
                ImGui.PushID(characterData.Identity);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(characterData.NameWithWorldCensored);
                ImGui.TableNextColumn();
                var plan = characterData.ExchangePlan == Guid.Empty ? null : C.AdditionalGCExchangePlans.FirstOrDefault(p => p.GUID == characterData.ExchangePlan);
                ImGui.SetNextItemWidth(200f);
                if(ImGui.BeginCombo("##chPlan", plan?.DisplayName ?? "Default Plan", ImGuiComboFlags.HeightLarge))
                {
                    if(ImGui.Selectable("Default Plan", plan == null)) characterData.ExchangePlan = Guid.Empty;
                    ImGui.Separator();
                    foreach(var exchangePlan in C.AdditionalGCExchangePlans)
                    {
                        ImGui.PushID(exchangePlan.ID);
                        if(ImGui.Selectable($"{exchangePlan.DisplayName}"))
                        {
                            characterData.ExchangePlan = exchangePlan.GUID;
                        }
                        ImGui.PopID();
                    }
                    ImGui.EndCombo();
                }
                ImGuiEx.DragDropRepopulate("Plan", plan?.GUID ?? Guid.Empty, ref characterData.ExchangePlan);

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo("##deliveryMode", ref characterData.GCDeliveryType);
                ImGuiEx.DragDropRepopulate("Mode", characterData.GCDeliveryType, ref characterData.GCDeliveryType);

                ImGui.PopID();
            }
            ImGui.EndTable();
        }
    }
}