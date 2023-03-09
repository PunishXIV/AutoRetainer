using AutoRetainer.Multi;
using AutoRetainer.NewScheduler;
using Dalamud.Game.ClientState.Conditions;
using ImGuiScene;
using System.IO;

namespace AutoRetainer.UI;

internal class MultiModeOverlay : Window
{
    public MultiModeOverlay() : base("AutoRetainer Alert", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |  ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground, true)
    {
        this.IsOpen = true;
        this.ShowCloseButton = false;
        this.RespectCloseHotkey = false;
    }

    public override void Draw()
    {
        CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());
        if (MultiMode.Enabled)
        {
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "multi.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, new(128, 128));
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        MultiMode.Enabled = false;
                    }
                    ImGui.SetTooltip("MultiMode enabled. \nLeft click - open AutoRetainer. \nRight click - disable Multi Mode.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading multi.png");
            }
        }
        else if(AutoLogin.Instance.IsRunning)
        {
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "login.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, new(128, 128));
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        AutoLogin.Instance.Abort();
                    }
                    ImGui.SetTooltip("Autologin is running, click to abort");
                }
            }
            else
            {
                ImGuiEx.Text($"loading login.png");
            }
        }
        else if (false && !P.configGui.IsOpen)
        {
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "bell.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, new(128, 128));
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        SchedulerMain.Enabled = false;
                    }
                    ImGui.SetTooltip("AutoRetainer enabled. \nLeft click - open AutoRetainer. \nRight click - disable AutoRetainer.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading bell.png");
            }
        }
        else if(P.config.NotifyEnableOverlay && NotificationHandler.CurrentState && !NotificationHandler.IsHidden && (!P.config.NotifyCombatDutyNoDisplay || !(Svc.Condition[ConditionFlag.BoundByDuty56] && Svc.Condition[ConditionFlag.InCombat])))
        {
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "notify.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, new(128, 128));
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        NotificationHandler.IsHidden = true;
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        NotificationHandler.IsHidden = true;
                    }
                    ImGui.SetTooltip("Some retainers completed their ventures. \nLeft click - open AutoRetainer;\nRight click - dismiss.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading notify.png");
            }
        }

        this.Position = new(ImGuiHelpers.MainViewport.Size.X/2 - ImGui.GetWindowSize().X/2, 20);
    }
}
