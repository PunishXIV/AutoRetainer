namespace AutoRetainer.UI.NeoUI.Experiments;
public class Notifications : ExperimentUIEntry
{
    public override void Draw()
    {
        ImGui.Checkbox($"Display overlay notification if one of retainers has completed a venture", ref C.NotifyEnableOverlay);
        ImGui.Checkbox($"Do not display overlay in duty or combat", ref C.NotifyCombatDutyNoDisplay);
        ImGui.Checkbox($"Include other characters", ref C.NotifyIncludeAllChara);
        ImGui.Checkbox($"Ignore other characters that have not been enabled in MultiMode", ref C.NotifyIgnoreNoMultiMode);
        ImGui.Checkbox($"Display notification in game chat", ref C.NotifyDisplayInChatX);
        ImGuiEx.Text($"If game is inactive: (requires NotificationMaster to be installed and enabled)");
        ImGui.Checkbox($"Send desktop notification on retainers available", ref C.NotifyDeskopToast);
        ImGui.Checkbox($"Flash taskbar", ref C.NotifyFlashTaskbar);
        ImGui.Checkbox($"Do not notify if AutoRetainer is enabled or MultiMode is running", ref C.NotifyNoToastWhenRunning);
    }
}
