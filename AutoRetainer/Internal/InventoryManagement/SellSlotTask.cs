using ECommons.ExcelServices;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Internal.InventoryManagement;

public class SellSlotTask
{
    public InventoryType InventoryType;
    public uint Slot;
    public uint ItemID;
    public uint Quantity;

    public SellSlotTask(InventoryType inventoryType, Number slot, Number itemID, Number quantity)
    {
        InventoryType = inventoryType;
        Slot = slot;
        ItemID = itemID;
        Quantity = quantity;
    }

    public override string ToString()
    {
        return $"[InventoryType={InventoryType},Slot={Slot},Item={ExcelItemHelper.GetName(ItemID, true)} x{Quantity}]";
    }
}
