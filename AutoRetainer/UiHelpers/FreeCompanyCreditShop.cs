using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UiHelpers;
public unsafe class FreeCompanyCreditShop : AddonMasterBase
{
    public FreeCompanyCreditShop(nint addon) : base(addon)
    {
    }

    public FreeCompanyCreditShop(void* addon) : base(addon)
    {
    }

    public override string AddonDescription { get; } = "";

    public void Buy(int index)
    {
        var evt = CreateAtkEvent();
        var data = CreateAtkEventData().Build();
        Addon->ReceiveEvent(AtkEventType.ListItemToggle, 0, &evt.CSEvent, &data);
    }
}
