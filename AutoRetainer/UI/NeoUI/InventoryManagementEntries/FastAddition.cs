using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class FastAddition : InventoryManagemenrBase
{
    public override string Name { get; } = "Fast Addition and Removal";

    private FastAddition()
    {
        this.NoFrame = false;
        this.DisplayPriority = -1;
    }

    public override void Draw()
    {
        ImGuiEx.TextWrapped(GradientColor.Get(EColor.RedBright, EColor.YellowBright), $"While this text is visible, hover over items while holding:");
        ImGuiEx.Text(!ImGui.GetIO().KeyShift ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Shift - add to Quick Venture Sell List");
        ImGuiEx.Text($"* Items that already in Unconditional Sell List WILL NOT BE ADDED to Quick Venture Sell List");
        ImGuiEx.Text(!ImGui.GetIO().KeyCtrl ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Ctrl - add to Unconditional Sell List");
        ImGuiEx.Text($"* Items that already in Quick Venture Sell List WILL BE MOVED to Unconditional Sell List");
        ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Alt - delete from either list");
        ImGuiEx.Text("\nItems that are protected are unaffected by these actions");
        if (Svc.GameGui.HoveredItem > 0)
        {
            var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
            if (ImGui.GetIO().KeyShift)
            {
                if (!C.IMProtectList.Contains(id) && !C.IMAutoVendorSoft.Contains(id) && !C.IMAutoVendorHard.Contains(id))
                {
                    C.IMAutoVendorSoft.Add(id);
                    Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Quick Venture Sell List");
                    C.IMAutoVendorHard.Remove(id);
                }
            }
            if (ImGui.GetIO().KeyCtrl)
            {
                if (!C.IMProtectList.Contains(id) && !C.IMAutoVendorHard.Contains(id) && !C.IMAutoVendorSoft.Contains(id))
                {
                    C.IMAutoVendorHard.Add(id);
                    Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Unconditional Sell List");
                    C.IMAutoVendorSoft.Remove(id);
                }
            }
            if (ImGui.GetIO().KeyAlt)
            {
                if (C.IMAutoVendorSoft.Remove(id)) Notify.Info($"Removed {ExcelItemHelper.GetName(id)} from Quick Venture Sell List");
                if (C.IMAutoVendorHard.Remove(id)) Notify.Info($"Removed {ExcelItemHelper.GetName(id)} from Unconditional Sell List");
            }
        }
    }
}
