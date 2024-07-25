using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class GeneralSettings : InventoryManagemenrBase
{
    public override string Name { get; } = "General Settings";

    private GeneralSettings()
    {
        this.Builder = new NuiBuilder()
            .Section(Name)
            .Checkbox($"Auto-open venture coffers", () => ref C.IMEnableCofferAutoOpen, "Multi Mode only. Before logging out, all coffers will be open unless your inventory space is too low.")
            .Checkbox($"Auto-vendor items", () => ref C.IMEnableAutoVendor)
            .Checkbox($"Auto-desynth items", () => ref C.IMEnableItemDesynthesis)
            .Checkbox($"Enable context menu integration", () => ref C.IMEnableContextMenu)
            .Checkbox($"Demo mode", () => ref C.IMDry, "Do not sell items, instead print in chat what would be sold")
            .Checkbox($"Treat soft list as hard list", () => ref C.TreatSoftAsHard);
    }
}
