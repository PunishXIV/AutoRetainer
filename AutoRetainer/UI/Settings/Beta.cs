using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.Settings;

internal static class Beta
{
    internal static void Draw()
    {
        ImGuiEx.Text(ImGuiColors.DalamudOrange, $"These features might be incomplete or cause minor problems.");
        InfoBox.DrawBox("Server time", delegate
        {
            ImGui.Checkbox($"Use server time instead of PC time", ref P.config.UseServerTime);
        });
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
