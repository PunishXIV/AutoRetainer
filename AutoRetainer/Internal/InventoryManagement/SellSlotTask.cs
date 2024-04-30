using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Internal.InventoryManagement
{
    public class SellSlotTask
    {
        public InventoryType InventoryType;
        public uint Slot;
        public uint ItemID;
        public uint Quantity;

        public SellSlotTask(InventoryType inventoryType, uint slot, uint itemID, uint quantity)
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
}
