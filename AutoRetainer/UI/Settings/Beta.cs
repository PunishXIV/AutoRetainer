namespace AutoRetainer.UI.Settings;

internal static class Beta
{
    internal static void Draw()
    {
        ImGuiEx.Text(ImGuiColors.DalamudOrange, $"These features might be incomplete, cause problems, or simply not work.");
        ImGui.Separator();
    }
}
