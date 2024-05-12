using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.GcHandin;
public unsafe class ReaderGrandCompanySupplyList(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
{
    uint LoadingStatus => ReadUInt(0) ?? 0;
    public bool IsLoaded => LoadingStatus == 2;
    public uint NumItems => ReadUInt(6) ?? 0;
}
