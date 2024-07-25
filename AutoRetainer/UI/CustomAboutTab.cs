using ECommons.Reflection;
using PunishLib;
using System.Diagnostics;

namespace AutoRetainer.UI
{
    public static class CustomAboutTab
    {
        static string GetImageURL()
        {
            return Svc.PluginInterface.Manifest.IconUrl ?? "";
        }

        public static void Draw()
        {
            ImGuiEx.LineCentered("About1", delegate
            {
                ImGuiEx.Text($"{Svc.PluginInterface.Manifest.Name} - {Svc.PluginInterface.Manifest.AssemblyVersion}");
            });

            ImGuiEx.LineCentered("About0", () =>
            {
                ImGuiEx.Text($"Published and developed with ");
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SameLine(0, 0);
                ImGuiEx.Text(ImGuiColors.DalamudRed, FontAwesomeIcon.Heart.ToIconString());
                ImGui.PopFont();
                ImGui.SameLine(0, 0);
                ImGuiEx.Text($" by Puni.sh and NightmareXIV");
            });
            
            ImGuiHelpers.ScaledDummy(10f);
            ImGuiEx.LineCentered("About2", delegate
            {
                if (ThreadLoadImageHandler.TryGetTextureWrap(GetImageURL(), out var texture))
                {
                    ImGui.Image(texture.ImGuiHandle, new(200f, 200f));
                }
            });
            ImGuiHelpers.ScaledDummy(10f);
            ImGuiEx.LineCentered("About3", delegate
            {
                ImGui.TextWrapped("Join our Discord community for project announcements, updates, and support.");
            });
            ImGuiEx.LineCentered("About4", delegate
            {
                if (ImGui.Button("Discord"))
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "https://discord.gg/Zzrcc8kmvy",
                        UseShellExecute = true
                    });
                }
                ImGui.SameLine();
                if (ImGui.Button("Repository"))
                {
                    ImGui.SetClipboardText("https://love.puni.sh/ment.json");
                    Notify.Success("Link copied to clipboard");
                }
                ImGui.SameLine();
                if (ImGui.Button("Source Code"))
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = Svc.PluginInterface.Manifest.RepoUrl,
                        UseShellExecute = true
                    });
                }
                ImGui.SameLine();
                if (ImGui.Button("Donate to Puni.sh platform"))
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "https://ko-fi.com/spetsnaz",
                        UseShellExecute = true
                    });
                }
            });
        }
    }
}
