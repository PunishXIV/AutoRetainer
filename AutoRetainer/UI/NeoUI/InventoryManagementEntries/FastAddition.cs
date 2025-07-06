using ECommons.ExcelServices;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class FastAddition : InventoryManagemenrBase
{
    public override string Name { get; } = "Automatic Selling/Fast Addition and Removal";

    private FastAddition()
    {
        NoFrame = false;
        DisplayPriority = -10;
    }

    public override void Draw()
    {
        var selectedSettings = Utils.GetSelectedIMSettings();
        ImGuiEx.TextWrapped(GradientColor.Get(EColor.RedBright, EColor.YellowBright), $"While this text is visible, hover over items while holding:");
        ImGuiEx.Text(!ImGui.GetIO().KeyShift ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Shift - add to Quick Venture Sell List");
        ImGuiEx.Text($"* Items that already in Unconditional Sell List WILL NOT BE ADDED to Quick Venture Sell List");
        ImGuiEx.Text(!ImGui.GetIO().KeyCtrl ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Ctrl - add to Unconditional Sell List");
        ImGuiEx.Text($"* Items that already in Quick Venture Sell List WILL BE MOVED to Unconditional Sell List");
        ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Alt - delete from either list");
        ImGuiEx.Text("\nItems that are protected are unaffected by these actions");
        if(Svc.GameGui.HoveredItem > 0)
        {
            var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
            if(ImGui.GetIO().KeyShift)
            {
                if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMAutoVendorSoft.Contains(id) && !selectedSettings.IMAutoVendorHard.Contains(id))
                {
                    selectedSettings.IMAutoVendorSoft.Add(id);
                    Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Quick Venture Sell List");
                    selectedSettings.IMAutoVendorHard.Remove(id);
                }
            }
            if(ImGui.GetIO().KeyCtrl)
            {
                if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMAutoVendorHard.Contains(id) && !selectedSettings.IMAutoVendorSoft.Contains(id))
                {
                    selectedSettings.IMAutoVendorHard.Add(id);
                    Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Unconditional Sell List");
                    selectedSettings.IMAutoVendorSoft.Remove(id);
                }
            }
            if(ImGui.GetIO().KeyAlt)
            {
                if(selectedSettings.IMAutoVendorSoft.Remove(id)) Notify.Info($"Removed {ExcelItemHelper.GetName(id)} from Quick Venture Sell List");
                if(selectedSettings.IMAutoVendorHard.Remove(id)) Notify.Info($"Removed {ExcelItemHelper.GetName(id)} from Unconditional Sell List");
            }
        }
    }
}
