namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class SoftList : InventoryManagemenrBase
{
    public override string Name => "Automatic Selling/Quick Venture Sell List";
    private SoftList()
    {
        var s = Utils.GetSelectedIMSettings();
        Builder = new NuiBuilder()
            .Section(Name)
            .TextWrapped("These items, when obtained from Quick Venture will be sold unless they have stacked with the same item.")
            .Widget(() => InventoryManagementCommon.DrawListNew(s.IMAutoVendorSoft))
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(s.IMAutoVendorSoft);
            });
    }
}
