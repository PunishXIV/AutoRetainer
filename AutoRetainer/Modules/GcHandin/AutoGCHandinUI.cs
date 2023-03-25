namespace AutoRetainer.Modules.GcHandin;

internal static class AutoGCHandinUI
{
    internal static void Draw()
    {
        ImGui.Checkbox("Tray notification upon handin completion (requires NotificationMaster)", ref P.config.GCHandinNotify);
    }
}
