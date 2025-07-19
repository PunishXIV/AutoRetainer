namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class HardList : InventoryManagemenrBase
{
    public override string Name => "Inventory Cleanup/Unconditional Sell List";

    private HardList()
    {
        var s = Utils.GetSelectedIMSettings();
        Builder = new NuiBuilder()
            .Section(Name)
            .TextWrapped("These items will always be sold, regardless of their source, as long as their stack count does not exceeds specified amount that you can specify below. Additionally, only these items will ever be sold to an NPC.")
            .InputInt(150f, $"Maximum stack size to be sold", () => ref s.IMAutoVendorHardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(s.IMAutoVendorHard, (x) =>
            {
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, s.IMAutoVendorHardIgnoreStack);
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Ignore stack setting for this item");
            }))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(s.IMAutoVendorHard);
            });
    }
}
