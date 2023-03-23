using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.Settings;

internal static class Beta
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
        ImGui.Checkbox("RetainerSense", ref P.config.RetainerSense);
        ImGuiComponents.HelpMarker($"Once you come near retainer bell and stay still for set amount of seconds, AutoRetainer will open the bell and enable itself if ventures are available");
        ImGui.SetNextItemWidth(200f);
        ImGuiEx.SliderIntAsFloat("Seconds to stay still before activation", ref P.config.RetainerSenseThreshold, 1000, 100000);
    }
}
