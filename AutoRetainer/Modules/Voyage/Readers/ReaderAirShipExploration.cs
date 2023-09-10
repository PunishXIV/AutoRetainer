using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Readers
{
    internal unsafe class ReaderAirShipExploration(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
    {
        internal string Fuel => ReadString(6);
        internal string Distance => ReadString(7);
        internal string ReturnsAt => ReadString(8);
        internal string VoyageTime => ReadString(9);

        internal List<Destination> Destinations => Loop<Destination>(13, 7, 74);

        internal unsafe class Destination(nint UnitBasePtr, int BeginOffset = 0) : AtkReader(UnitBasePtr, BeginOffset)
        {
            internal uint Unknown0 => ReadUInt(0) ?? 0;
            internal string NameFull => ReadSeString(1).ExtractText().Trim();
            internal string NameShort => ReadSeString(2).ExtractText().Trim();
            internal uint Unknown3 => ReadUInt(3) ?? 0;
            internal uint RequiredRank => ReadUInt(4) ?? uint.MaxValue;
            internal uint Unknown5 => ReadUInt(5) ?? 0;
            internal uint StatusFlag => ReadUInt(6) ?? uint.MaxValue;

            internal bool CanBeSelected => StatusFlag.EqualsAny<uint>(0,1);

            public override string ToString()
            {
                return $"(\"{NameFull}\"/\"{NameShort}\", RequiredRank={RequiredRank}, StatusFlag={StatusFlag}, CanBeSelected={CanBeSelected})";
            }
        }
    }
}
