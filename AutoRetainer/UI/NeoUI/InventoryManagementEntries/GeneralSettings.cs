using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;
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
            .Checkbox($"Enable selling items to retainer", () => ref C.IMEnableAutoVendor)
            .Indent()
            .Checkbox($"Also allow selling items to housing NPC (beta)", () => ref C.IMEnableNpcSell, "Place any shop NPC in a way that you can interact with it after entering the house")
            .Indent()
            .Widget("Sell now", (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && C.IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent();
                }
            })
            .Unindent()
            .Checkbox($"Auto-desynth items", () => ref C.IMEnableItemDesynthesis)
            .Checkbox($"Enable context menu integration", () => ref C.IMEnableContextMenu)
            .Checkbox($"Demo mode", () => ref C.IMDry, "Do not sell items, instead print in chat what would be sold")
            .Unindent()
            ;
    }
}
