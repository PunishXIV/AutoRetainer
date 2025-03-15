using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.GcHandin;
public unsafe class ReaderGrandCompanyExchange(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
{
    public uint ItemCount => ReadUInt(1) ?? 0;
    public List<ItemInfo> Items => Loop<ItemInfo>(17, 1, (int)ItemCount);

    public unsafe class ItemInfo(nint UnitBasePtr, int BeginOffset = 0) : AtkReader(UnitBasePtr, BeginOffset)
    {
        public string Name => ReadSeString(0)?.GetText();
        public uint Seals => ReadUInt(50) ?? 0;
        public uint Bag => ReadUInt(100) ?? 0;
        public uint IconID => ReadUInt(150) ?? 0;
        public bool Unk250 => ReadBool(250) ?? false;
        public uint ItemID => ReadUInt(300) ?? 0;
        public uint Unk350 => ReadUInt(350) ?? 0;
        public uint RankReq => ReadUInt(400) ?? 0;
        public bool OpenCurrencyExchange => ReadBool(450) ?? false;
    }
}
