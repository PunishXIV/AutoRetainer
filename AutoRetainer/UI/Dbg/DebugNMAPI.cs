using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Dbg
{
    internal static class DebugNMAPI
    {
        static float vol;
        static bool repeat;
        static bool stopOnFocus;
        static string path = "";
        internal static void Draw()
        {
            ImGuiEx.Text($"Active: {P.NotificationMasterApi.IsIPCReady()}");
            ImGui.InputText("path", ref path, 500);
            ImGui.InputFloat("vol", ref vol);
            ImGui.Checkbox("repeat", ref repeat);
            ImGui.Checkbox("stopOnFocus", ref stopOnFocus);
            if (ImGui.Button("Flash")) new TickScheduler(() => P.NotificationMasterApi.FlashTaskbarIcon(), 1000);
            if (ImGui.Button("msg")) new TickScheduler(() => P.NotificationMasterApi.DisplayTrayNotification("Title", "Text"), 1000);
            if (ImGui.Button("msg no title")) new TickScheduler(() => P.NotificationMasterApi.DisplayTrayNotification("Text"), 1000);
            if (ImGui.Button("play sound")) new TickScheduler(() => P.NotificationMasterApi.PlaySound(path, vol, repeat, stopOnFocus), 1000);
            if (ImGui.Button("stop sound")) P.NotificationMasterApi.StopSound();
        }
    }
}
