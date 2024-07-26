namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class DebugNMAPI : DebugSectionBase
{
    private static float vol;
    private static bool repeat;
    private static bool stopOnFocus;
    private static string path = "";
    public override void Draw()
    {
        ImGuiEx.Text($"Active: {P.NotificationMasterApi.IsIPCReady()}");
        ImGui.InputText("path", ref path, 500);
        ImGui.InputFloat("vol", ref vol);
        ImGui.Checkbox("repeat", ref repeat);
        ImGui.Checkbox("stopOnFocus", ref stopOnFocus);
        if(ImGui.Button("Flash")) new TickScheduler(() => P.NotificationMasterApi.FlashTaskbarIcon(), 1000);
        if(ImGui.Button("msg")) new TickScheduler(() => P.NotificationMasterApi.DisplayTrayNotification("Title", "Text"), 1000);
        if(ImGui.Button("msg no title")) new TickScheduler(() => P.NotificationMasterApi.DisplayTrayNotification("Text"), 1000);
        if(ImGui.Button("play sound")) new TickScheduler(() => P.NotificationMasterApi.PlaySound(path, vol, repeat, stopOnFocus), 1000);
        if(ImGui.Button("stop sound")) P.NotificationMasterApi.StopSound();
    }
}
