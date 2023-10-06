using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.Settings;

internal static class SuperSecret
{
    internal static void Draw()
    {
        ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "Anything can happen here.");
        InfoBox.DrawBox("Notification settings", NotifyGui.Draw);
        ImGui.Checkbox("Old RetainerSense", ref C.OldRetainerSense);
        ImGuiComponents.HelpMarker("Detect and use the closest Summoning Bell within valid distance of the player.");
        ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "RetainerSense is enforced to be active during MultiMode operation.");
        ImGui.Separator();
        ImGui.Checkbox($"Unsafe options protection", ref C.UnsafeProtection);
        ImGui.SameLine();
        if (ImGui.Button($"Write to registry"))
        {
            Safety.Set(C.UnsafeProtection);
        }
        var g = Safety.Get();
        ImGuiEx.Text(g?ImGuiColors.ParsedGreen:ImGuiColors.DalamudRed, $"Safety flag: {(g ? "Present" : "Absent")}");
    }
}
