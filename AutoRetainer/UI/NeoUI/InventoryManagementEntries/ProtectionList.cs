using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class ProtectionList : InventoryManagemenrBase
{
    public override string Name { get; } = "Protection List";

    private ProtectionList()
    {
        this.Builder = new NuiBuilder()
            .Section(Name)
            .TextWrapped("AutoRetainer won't sell, desynthese, discard or hand in to Grand Company these items, even if they are included in any other processing lists.")
            .Widget(() => InventoryManagementCommon.DrawListNew(C.IMProtectList))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportBlacklistFromArDiscard();
            });
    }

}