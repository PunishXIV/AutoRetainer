using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class GeneralSettings : InventoryManagemenrBase
{
    public override string Name { get; } = "Inventory Cleanup/General Settings";

    private GeneralSettings()
    {
        Builder = new NuiBuilder()
            .Section(Name)
            .Checkbox($"Auto-open venture coffers", () => ref Utils.GetSelectedIMSettings().IMEnableCofferAutoOpen, "Multi Mode only. Before logging out, all coffers will be open unless your inventory space is too low.")
            .Checkbox($"Enable selling items to retainer", () => ref Utils.GetSelectedIMSettings().IMEnableAutoVendor)
            .Checkbox($"Enable selling items to housing NPC", () => ref Utils.GetSelectedIMSettings().IMEnableNpcSell, "Place any shop NPC in a way that you can interact with it after entering the house")
            .Indent()
            .Checkbox($"Ignore NPC if retainer is available", () => ref Utils.GetSelectedIMSettings().IMSkipVendorIfRetainer)
            .Widget("Sell now", (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && Utils.GetSelectedIMSettings().IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent(true);
                }
            })
            .Unindent()
            .Checkbox($"Auto-desynth items", () => ref Utils.GetSelectedIMSettings().IMEnableItemDesynthesis)
            .Checkbox($"Enable context menu integration", () => ref Utils.GetSelectedIMSettings().IMEnableContextMenu)
            .Checkbox($"Allow selling items from Armory Chest", () => ref Utils.GetSelectedIMSettings().AllowSellFromArmory)
            .Checkbox($"Demo mode", () => ref Utils.GetSelectedIMSettings().IMDry, "Do not sell items, instead print in chat what would be sold")
            ;
    }
}
