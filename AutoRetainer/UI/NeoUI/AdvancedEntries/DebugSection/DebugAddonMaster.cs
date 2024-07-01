using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public unsafe class DebugAddonMaster : DebugUIEntry
{
    public override void Draw()
    {
        if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && IsAddonReady(addon))
        {
            var r = new AddonMaster.RetainerList(addon);
            foreach(var x in r.Retainers)
            {
                ImGuiEx.Text($"{x.Name}, {x.IsActive}");
                if (ImGuiEx.HoveredAndClicked())
                {
                    x.Select();
                }
            }
        }
    }
}
