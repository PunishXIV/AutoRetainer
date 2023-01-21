using AutoRetainer.GcHandin;
using Dalamud.Interface.Style;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI
{
    internal static class TabBeta
    {
        internal static void Draw()
        {
            ImGuiEx.Text(ImGuiColors.DalamudOrange, $"These features might be incomplete or cause minor problems.");
            InfoBox.DrawBox("Auto GC Expert Delivery", AutoGCHandinUI.Draw);
            InfoBox.DrawBox("House Enter Task", delegate
            {
                ImGui.Checkbox($"Multi Mode: support housing retainer bells", ref P.config.MultiAllowHET);
                ImGuiEx.TextWrapped(ImGuiColors.DalamudOrange, $"Retainer bell must be within reach after you enter the house.");
            });
            if (P.config.SS)
            {
                InfoBox.DrawBox("Notification settings", NotifyGui.Draw);
            }
        }
    }
}
