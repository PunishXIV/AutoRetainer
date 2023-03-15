namespace AutoRetainer.UI.Settings;

internal static class NotifyGui
{
    internal static void Draw()
    {
        ImGui.Checkbox($"Display overlay notification if one of retainers has completed a venture", ref P.config.NotifyEnableOverlay);
        ImGui.Checkbox($"Do not display overlay in duty or combat", ref P.config.NotifyCombatDutyNoDisplay);
        ImGui.Checkbox($"Include other characters", ref P.config.NotifyIncludeAllChara);
        ImGui.Checkbox($"Ignore other characters that have not been enabled in MultiMode", ref P.config.NotifyIgnoreNoMultiMode);
        ImGui.Checkbox($"Display notification in game chat", ref P.config.NotifyDisplayInChatX);
        ImGuiEx.Text($"If game is inactive: (requires NotificationMaster to be installed and enabled)");
        ImGui.Checkbox($"Send desktop notification on retainers available", ref P.config.NotifyDeskopToast);
        ImGui.Checkbox($"Flash taskbar", ref P.config.NotifyFlashTaskbar);
        ImGui.Checkbox($"Do not notify if AutoRetainer is enabled or MultiMode is running", ref P.config.NotifyNoToastWhenRunning);
    }
}
