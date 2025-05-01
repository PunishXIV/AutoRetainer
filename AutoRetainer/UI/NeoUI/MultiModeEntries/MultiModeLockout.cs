using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeLockout : NeoUIEntry
{
    public override string Path => "Multi Mode/Region Lock";

    private int Num = 12;

    public override void Draw()
    {
        ImGuiEx.TextV("For");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);
        ImGui.InputInt("hours...", ref Num.ValidateRange(1, 10000));
        foreach(var x in Enum.GetValues<ExcelWorldHelper.Region>())
        {
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Lock, $"...do not log into {x} region"))
            {
                C.LockoutTime[x] = DateTimeOffset.Now.ToUnixTimeSeconds() + Num * 60 * 60;
            }
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Unlock, "Remove all locks"))
        {
            C.LockoutTime.Clear();
        }
    }
}
