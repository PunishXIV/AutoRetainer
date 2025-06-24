using AutoRetainerAPI.Configuration;
using Lumina.Excel.Sheets;
using GrandCompanyRank = Lumina.Excel.Sheets.GrandCompanyRank;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public unsafe sealed class ExchangeList : InventoryManagemenrBase
{
    ImGuiEx.RealtimeDragDrop<ItemWithQuantity> DragDrop = new("GCELDD", x => x.ID);
    public override string Name { get; } = "Grand Company Delivery/Exchange List";

    public override void Draw()
    {
        ImGuiEx.TextWrapped($"""
            Select which items you'd like to buy during automatic Grand Company expert delivery missions.
            First available item for purchase will be bought until it's amount in your inventory reaches the target. If there is nothing else to be purchased or items don't fit into your inventory, ventures will be purchased until you reach 65000 of them, at which point expert delivery will discard excess seals.
            """);
        DrawGCEchangeList(C.DefaultGCExchangePlan);
    }

    void DrawGCEchangeList(GCExchangePlan plan)
    {
        ImGui.PushID(plan.ID);  
        plan.Validate();
        ref var filter = ref Ref<string>.Get($"{plan.ID}filter");
        ref var onlySelected = ref Ref<bool>.Get($"{plan.ID}onlySel");
        ImGui.Checkbox("Only Selected", ref onlySelected);
        ImGui.SameLine();
        ImGuiEx.SetNextItemFullWidth();
        ImGui.InputTextWithHint("##filter", "Search...", ref filter, 100);
        DragDrop.Begin();
        if(ImGuiEx.BeginDefaultTable("GCDeliveryList", ["##dragDrop", "~Item", "Price", "Category", "Amount"]))
        {
            for(var i = 0; i < plan.Items.Count; i++)
            {
                var x = plan.Items[i];
                var meta = Utils.SharedGCExchangeListings[x.ItemID];
                if(onlySelected && x.Quantity == 0) continue;
                if(filter.Length > 0 
                    && !meta.Data.Name.GetText().Contains(filter, StringComparison.OrdinalIgnoreCase)
                    && !meta.Category.ToString().Equals(filter, StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[meta.MinPurchaseRank].Equals(filter, StringComparison.OrdinalIgnoreCase)
                    ) continue;
                ImGui.PushID(x.ID);
                ImGui.TableNextRow();
                DragDrop.SetRowColor(x);
                ImGui.TableNextColumn();
                DragDrop.NextRow();
                DragDrop.DrawButtonDummy(x, plan.Items, i);
                ImGui.TableNextColumn();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(meta.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.ImGuiHandle, new(ImGui.GetFrameHeight()));
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Data.Name.GetText()}");
                ImGui.TableNextColumn();
                if(Svc.Data.GetExcelSheet<GrandCompanyRank>().TryGetRow(meta.MinPurchaseRank, out var rank) && ThreadLoadImageHandler.TryGetIconTextureWrap(rank.IconFlames, false, out var tex))
                {
                    ImGui.Image(tex.ImGuiHandle, new(ImGui.GetFrameHeight()));
                    var rankName = Utils.GCRanks[meta.MinPurchaseRank];
                    ImGuiEx.Tooltip(rankName);
                    if(ImGuiEx.HoveredAndClicked()) filter = rankName;
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Seals}");
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Category}");
                if(ImGuiEx.HoveredAndClicked()) filter = meta.Category.ToString();
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(150f.Scale());
                ImGui.InputInt("##qty", ref x.Quantity.ValidateRange(0, int.MaxValue), 1, 10);
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        DragDrop.End();
        ImGui.PopID();
    }
}