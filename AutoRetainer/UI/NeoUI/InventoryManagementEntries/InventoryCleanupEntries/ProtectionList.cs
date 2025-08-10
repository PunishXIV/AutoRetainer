namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class ProtectionList : InventoryManagementBase
{
    public override string Name { get; } = "Inventory Cleanup/Protection List";
    private InventoryManagementCommon InventoryManagementCommon = new();
    private ProtectionList()
    {
        DisplayPriority = -1;
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("AutoRetainer won't sell, desynthese, discard or hand in to Grand Company these items, even if they are included in any other processing lists.")
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.Protect, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMProtectList.Remove(itemId), InventoryCleanupCommon.SelectedPlan.IMProtectList))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportBlacklistFromArDiscard();
            });
    }

}