namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class SoftList : InventoryManagemenrBase
{
    public override string Name => "Inventory Cleanup/Quick Venture Sell List";
    private SoftList()
    {
        var s = InventoryCleanupCommon.SelectedPlan;
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("These items, when obtained from Quick Venture will be sold unless they have stacked with the same item.")
            .Widget(() => InventoryManagementCommon.DrawListNew(s.IMAutoVendorSoft))
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(s.IMAutoVendorSoft);
            });
    }
}
