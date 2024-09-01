namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class SoftList : InventoryManagemenrBase
{
    public override string Name => "Quick Venture Sell List";
    private SoftList()
    {
        Builder = new NuiBuilder()
            .Section(Name)
            .TextWrapped("These items, when obtained from Quick Venture will be sold unless they have stacked with the same item.")
            .Widget(() => InventoryManagementCommon.DrawListNew(C.IMAutoVendorSoft))
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(C.IMAutoVendorSoft);
            });
    }
}
