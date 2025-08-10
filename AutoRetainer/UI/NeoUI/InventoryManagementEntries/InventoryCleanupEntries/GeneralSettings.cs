using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "Inventory Cleanup/General Settings";

    private GeneralSettings()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .Checkbox($"Auto-open venture coffers", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableCofferAutoOpen, "Multi Mode only. Before logging out, all coffers will be opened unless your inventory space is too low.")
            .Checkbox($"Enable selling items to retainer", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableAutoVendor, "When AutoRetainer checks resents retainers to ventures, items will be sold according to Inventory Cleanup plan.")
            .Checkbox($"Enable selling items to housing NPC", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell, "When AutoRetainer enters a house, items will be sold according to the Inventory Cleanup plan. A housing vendor that supports item selling must be placed near the house entrance (not the workshop entrance)—you should be able to interact with the NPC immediately after entering.")
            .Indent()
            .Checkbox($"Ignore NPC if retainer is available", () => ref InventoryCleanupCommon.SelectedPlan.IMSkipVendorIfRetainer)
            .Widget("Sell now", (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent(true);
                }
            })
            .Unindent()
            .Checkbox($"Auto-desynth items", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesis)
            .Checkbox($"Enable context menu integration", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableContextMenu)
            .Checkbox($"Allow selling items from Armory Chest", () => ref InventoryCleanupCommon.SelectedPlan.AllowSellFromArmory)
            .Checkbox($"Demo mode", () => ref InventoryCleanupCommon.SelectedPlan.IMDry, "Do not sell items, instead print in chat what would be sold")
            ;
    }
}
