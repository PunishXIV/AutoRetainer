﻿using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Reflection;

namespace AutoRetainer.UiHelpers;
public unsafe class ReaderFreeCompanyCreditShop : AtkReader
{
    public ReaderFreeCompanyCreditShop(AtkUnitBase* UnitBase, int BeginOffset = 0) : base(UnitBase, BeginOffset)
    {
    }

    public ReaderFreeCompanyCreditShop(nint UnitBasePtr, int BeginOffset = 0) : base(UnitBasePtr, BeginOffset)
    {
    }
    public uint FCRank => ReadUInt(0) ?? 0;
    public uint Credits => ReadUInt(3) ?? 0;
    public uint Count => ReadUInt(9) ?? 0;
    public List<Listing> Listings => Loop<Listing>(10, 1, (int)Count);

    public class Listing : AtkReader
    {
        public Listing(AtkUnitBase* UnitBase, int BeginOffset = 0) : base(UnitBase, BeginOffset)
        {
        }

        public Listing(nint UnitBasePtr, int BeginOffset = 0) : base(UnitBasePtr, BeginOffset)
        {
        }

        public string Name => ReadSeString(0)?.GetText();
        public uint ItemID => ReadUInt(20) ?? 0;
        public int IconID => ReadInt(40) ?? 0;
        public uint RankReq => ReadUInt(60) ?? 0;
        public uint InInventory => ReadUInt(80) ?? 0;
        public int MaxStack => ReadInt(100) ?? 0;
        public uint Price => ReadUInt(120) ?? 0;

        public override string ToString()
        {
            return GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(x => $"{x.Name}=" + (x.GetValue(this)?.ToString() ?? "<null>")).Print(", ");
        }
    }
}
