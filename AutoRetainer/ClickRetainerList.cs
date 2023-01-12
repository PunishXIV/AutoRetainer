using ClickLib.Enums;
using ClickLib.Structures;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer;

internal unsafe class ClickRetainerList : ClickLib.Bases.ClickBase<ClickRetainerList, AtkUnitBase>
{
    public ClickRetainerList(IntPtr addon = default)
        : base("RetainerList", addon)
    {
    }
    public static implicit operator ClickRetainerList(IntPtr addon) => new(addon);

    public static ClickRetainerList Using(IntPtr addon) => new(addon);

    public void Select(void* list, AtkComponentNode* target, uint index)
    {
        var data = InputData.Empty();
        data.Data[0] = target;
        data.Data[2] = (void*)(index | ((ulong)index << 48));
        ClickAddonComponent(target, 1, EventType.LIST_INDEX_CHANGE, null, data);
    }
}
