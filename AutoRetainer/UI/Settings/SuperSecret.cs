using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Settings
{
    internal static class SuperSecret
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "Anything can happen here.");
            InfoBox.DrawBox("Notification settings", NotifyGui.Draw);
            ImGui.Checkbox("Old RetainerSense", ref P.config.OldRetainerSense);
            ImGuiComponents.HelpMarker("Detect and use the closest Summoning Bell within valid distance of the player.");
            ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "RetainerSense is enforced to be active during MultiMode operation.");
        }
    }
}
