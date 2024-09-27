using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UiHelpers;
public unsafe class ReaderFreeCompanyCreditShop : AtkReader
{
    public ReaderFreeCompanyCreditShop(AtkUnitBase* UnitBase, int BeginOffset = 0) : base(UnitBase, BeginOffset)
    {
    }

    public ReaderFreeCompanyCreditShop(nint UnitBasePtr, int BeginOffset = 0) : base(UnitBasePtr, BeginOffset)
    {
    }
    public uint FCRank => this.ReadUInt(0) ?? 0;
    public uint Credits => this.ReadUInt(3) ?? 0;
    public uint Count => this.ReadUInt(9) ?? 0;
    public List<Listing> Listings => Loop<Listing>(10, 1, (int)Count);

    public class Listing : AtkReader
    {
        public Listing(AtkUnitBase* UnitBase, int BeginOffset = 0) : base(UnitBase, BeginOffset)
        {
        }

        public Listing(nint UnitBasePtr, int BeginOffset = 0) : base(UnitBasePtr, BeginOffset)
        {
        }

        public string Name => ReadSeString(0)?.ExtractText();
        public uint ItemID => ReadUInt(20) ?? 0;
        public int IconID => ReadInt(40) ?? 0;
        public uint RankReq => ReadUInt(60) ?? 0;
        public uint InInventory => ReadUInt(80) ?? 0;
        public int MaxStack => ReadInt(100) ?? 0;
        public uint Price => ReadUInt(120) ?? 0;

        public override string ToString()
        {
            return this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(x => $"{x.Name}=" + (x.GetValue(this)?.ToString() ?? "<null>")).Print(", ");
        }
    }
}
