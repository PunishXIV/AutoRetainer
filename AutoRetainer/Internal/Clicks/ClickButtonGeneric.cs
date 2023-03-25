using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Internal.Clicks;

internal unsafe class ClickButtonGeneric : ClickLib.Bases.ClickBase<ClickButtonGeneric, AtkUnitBase>
{
    internal string Name;
    public ClickButtonGeneric(void* addon, string name)
    : base(name, (nint)addon)
    {
        Name = name;
    }

    public void Click(void* target, uint which = 0)
    {
        ClickAddonButton((AtkComponentButton*)target, which);
    }
}
