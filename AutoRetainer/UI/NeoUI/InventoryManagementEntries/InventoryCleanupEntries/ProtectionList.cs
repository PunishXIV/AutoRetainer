namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class ProtectionList : InventoryManagemenrBase
{
    public override string Name { get; } = "Inventory Cleanup/Protection List";

    private ProtectionList()
    {
        DisplayPriority = -1;
        Builder = new NuiBuilder()
            .Section(Name)
            .TextWrapped("AutoRetainer won't sell, desynthese, discard or hand in to Grand Company these items, even if they are included in any other processing lists.")
            .Widget(() => InventoryManagementCommon.DrawListNew(Utils.GetSelectedIMSettings().IMProtectList))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportBlacklistFromArDiscard();
            });
    }

}