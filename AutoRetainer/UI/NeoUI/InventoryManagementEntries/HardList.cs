using ECommons.Configuration;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class HardList : InventoryManagemenrBase
{
    public override string Name => "Unconditional Sell List";

    private HardList()
    {
        this.Builder = new NuiBuilder()
            .Section(Name)
            .TextWrapped("These items will always be sold, regardless of their source, as long as their stack count does not exceeds specified amount that you can specify below. Additionally, only these items will ever be sold to an NPC.")
            .InputInt(150f, $"Maximum stack size to be sold", () => ref C.IMAutoVendorHardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(C.IMAutoVendorHard, (x) =>
            {
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, C.IMAutoVendorHardIgnoreStack);
                ImGui.PopFont();
                ImGuiEx.Tooltip($"Ignore stack setting for this item");
            }))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(C.IMAutoVendorHard);
            });
    }
}
