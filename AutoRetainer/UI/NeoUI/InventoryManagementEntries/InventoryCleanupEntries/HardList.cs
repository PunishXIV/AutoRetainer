namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class HardList : InventoryManagementBase
{
    public override string Name => "Inventory Cleanup/Unconditional Sell List";
    private InventoryManagementCommon InventoryManagementCommon = new();

    private HardList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("These items will always be sold, regardless of their source, as long as their stack count does not exceeds specified amount that you can specify below. Additionally, only these items will ever be sold to an NPC.")
            .InputInt(150f, $"Maximum stack size to be sold", () => ref InventoryCleanupCommon.SelectedPlan.IMAutoVendorHardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.HardSell, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard.Remove(itemId),
                InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard, 
                (x) =>
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, InventoryCleanupCommon.SelectedPlan.IMAutoVendorHardIgnoreStack);
                    ImGui.PopFont();
                    ImGuiEx.Tooltip($"Ignore stack setting for this item");
                },
                filter: item => item.PriceLow != 0))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard);
            });
    }
}
