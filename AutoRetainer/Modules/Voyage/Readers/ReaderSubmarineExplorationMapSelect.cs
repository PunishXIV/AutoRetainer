using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Readers;

internal unsafe class ReaderSubmarineExplorationMapSelect(AtkUnitBase* UnitBase) : AtkReader(UnitBase)
{
    internal uint SubmarineRank => ReadUInt(1) ?? 0;
    internal List<Map> Maps => Loop<Map>(3, 3, 6);

    internal unsafe class Map(nint UnitBasePtr, int offset = 0) : AtkReader(UnitBasePtr, offset)
    {
        internal uint Id => ReadUInt(0) ?? 0;
        internal string Name => ReadString(1);
        internal uint RequiredRank => ReadUInt(2) ?? uint.MaxValue;
    }
}
