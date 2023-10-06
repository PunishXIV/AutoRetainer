using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Internal
{
    internal unsafe class ReaderRetainerItemTransferList(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
    {
        internal List<Item> Items => Loop<Item>(1, 1, 140);

        internal unsafe class Item(nint UnitBasePtr, int BeginOffset = 0) : AtkReader(UnitBasePtr, BeginOffset)
        {
            internal uint ItemIDRaw => ReadUInt(0) ?? 0;
            internal uint ItemID => ItemIDRaw > 1000000 ? ItemIDRaw - 1000000 : ItemIDRaw;
            internal bool IsHQ => ItemIDRaw > 1000000;
        }
    }
}
