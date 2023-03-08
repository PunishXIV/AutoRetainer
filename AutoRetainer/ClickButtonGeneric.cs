using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer
{
    internal unsafe class ClickButtonGeneric : ClickLib.Bases.ClickBase<ClickButtonGeneric, AtkUnitBase>
    {
        internal string Name;
        public ClickButtonGeneric(void* addon, string name)
        : base(name, (nint)addon)
        {
            this.Name = name;
        }

        public void Click(void* target, uint which = 0)
        {
            this.ClickAddonButton((AtkComponentButton*)target, which);
        }
    }
}
