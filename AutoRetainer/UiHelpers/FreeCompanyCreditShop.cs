using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UiHelpers;
public unsafe class FreeCompanyCreditShop : AddonMasterBase
{
    public FreeCompanyCreditShop(nint addon) : base(addon)
    {
    }

    public FreeCompanyCreditShop(void* addon) : base(addon)
    {
    }

    public void Buy(int index)
    {
        var evt = this.CreateAtkEvent();
        var data = this.CreateAtkEventData().Build();
        Addon->ReceiveEvent(AtkEventType.ListItemToggle, 0, &evt, &data);
    }
}
