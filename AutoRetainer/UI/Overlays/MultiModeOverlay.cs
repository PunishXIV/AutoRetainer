using AutoRetainer.Modules.Voyage;
using Dalamud.Game.ClientState.Conditions;
using System.IO;

namespace AutoRetainer.UI.Overlays;

internal class MultiModeOverlay : Window
{
    public MultiModeOverlay() : base("AutoRetainer Alert", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground, true)
		{
				P.WindowSystem.AddWindow(this);
				IsOpen = true;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
    }

    bool DisplayNotify => C.NotifyEnableOverlay && NotificationHandler.CurrentState && !NotificationHandler.IsHidden && (!C.NotifyCombatDutyNoDisplay || !(Svc.Condition[ConditionFlag.BoundByDuty56] && Svc.Condition[ConditionFlag.InCombat]));

    public override bool DrawConditions()
    {
        return !C.HideOverlayIcons && (P.TaskManager.IsBusy || P.IsNextToBell || MultiMode.Enabled || AutoLogin.Instance.IsRunning || SchedulerMain.PluginEnabled || DisplayNotify || VoyageScheduler.Enabled || Shutdown.Active);
    }

    Vector2 StatusPanelSize => new(C.StatusBarIconWidth);

    public override void Draw()
    {
        var displayed = false;
        bool ShouldDisplay() => !displayed || !C.StatusBarMSI;
        CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());
        if (BailoutManager.IsLogOnTitleEnabled && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "bailoutTitleRestart.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        BailoutManager.IsLogOnTitleEnabled = false;
                    }
                    ImGui.SetTooltip($"AutoRetainer was requested to temporarily wait for valid character on login screen. \nLeft click - open AutoRetainer. \nRight click - abort.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading bailoutTitleRestart.png");
            }
            ImGui.SameLine();
        }
        if (Shutdown.Active && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "timer.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        Shutdown.ForceShutdownAt = 0;
                        Shutdown.ShutdownAt = 0;
                    }
                    ImGui.SetTooltip($"A shutdown timer is set.\nShutting down in {TimeSpan.FromMilliseconds(Shutdown.ShutdownAt - Environment.TickCount64)}\nForce shutdown in {TimeSpan.FromMilliseconds(Shutdown.ForceShutdownAt - Environment.TickCount64)} \nLeft click - open AutoRetainer. \nRight click - clear timer.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading timer.png");
            }
            ImGui.SameLine();
        }
        if (P.TaskManager.IsBusy && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "processing.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        P.TaskManager.Abort();
                    }
                    ImGui.SetTooltip("AutoRetainer is processing tasks. \nLeft click - open AutoRetainer. \nRight click - abort.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading multi.png");
            }
            ImGui.SameLine();
        }
        if (P.IsNextToBell && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "bellalert.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    ImGui.SetTooltip("RetainerSense is active. \nLeft click - open AutoRetainer.");
                }
                var f = (float)(Environment.TickCount64 - P.LastMovementAt) / (float)C.RetainerSenseThreshold;
                ImGui.ProgressBar(f, new(128, 10), "");
            }
            else
            {
                ImGuiEx.Text($"loading bellalert.png");
            }
            ImGui.SameLine();
        }
        if (MultiMode.Enabled && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "multi.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
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
            ImGui.SameLine();
        }
        if (C.NightMode && MultiMode.Enabled && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "Night.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        C.NightMode = false;
                        MultiMode.BailoutNightMode();
                    }
                    ImGui.SetTooltip($"Night mode enabled. \nLeft click - open AutoRetainer. \nRight click - disable.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading Night.png");
            }
            ImGui.SameLine();
        }
        if (AutoLogin.Instance.IsRunning && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "login.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        AutoLogin.Instance.Abort();
                    }
                    ImGui.SetTooltip("Autologin is running.\nLeft click - open AutoRetainer. \nRight click - disable Multi Mode.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading login.png");
            }
            ImGui.SameLine();
        }
        if (VoyageScheduler.Enabled && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "submarine.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        VoyageScheduler.Enabled = false;
                    }
                    ImGui.SetTooltip("Submarine module enabled. \nLeft click - open AutoRetainer. \nRight click - disable submarine module.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading submarine.png");
            }
            ImGui.SameLine();
        }
        if (SchedulerMain.PluginEnabled && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", Utils.GetReachableRetainerBell(false) == null ? "bellcrossed.png" : "bell.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        SchedulerMain.DisablePlugin();
                    }
                    ImGui.SetTooltip("AutoRetainer enabled. \nLeft click - open AutoRetainer. \nRight click - disable AutoRetainer.");
                }
            }
            else
            {
                ImGuiEx.Text($"loading bell.png");
            }
            ImGui.SameLine();
        }
        if (DisplayNotify && ShouldDisplay())
        {
            displayed = true;
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "notify.png"), out var t))
            {
                ImGui.Image(t.ImGuiHandle, StatusPanelSize);
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
            ImGui.SameLine();
        }
        ImGui.Dummy(Vector2.One);
        if (Data != null && !C.OldStatusIcons)
        {
            ImGuiEx.ImGuiLineCentered("Status", delegate
            {
                if (C.MultiModeWorkshopConfiguration.MultiWaitForAll)
                {
                    if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "wait.png"), out var t))
                    {
                        ImGui.Image(t.ImGuiHandle, StatusPanelSize / 2);
                        ImGuiEx.Tooltip("Wait for all deployables is globally enabled.");
                    }
                    else
                    {
                        ImGuiEx.Text($"loading wait.png");
                    }
                    ImGui.SameLine();
                }
                else if (Data.MultiWaitForAllDeployables)
                {
                    if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "wait.png"), out var t))
                    {
                        ImGui.Image(t.ImGuiHandle, StatusPanelSize / 2);
                        ImGuiEx.Tooltip("Wait for all deployables is enabled for this character.");
                    }
                    else
                    {
                        ImGuiEx.Text($"loading wait.png");
                    }
                    ImGui.SameLine();
                }

                if (Data.TeleportToFCHouse || Data.TeleportToRetainerHouse)
                {
                    if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "home.png"), out var t))
                    {
                        ImGui.Image(t.ImGuiHandle, StatusPanelSize / 2);
                        ImGuiEx.Tooltip("This character is allowed to teleport to house upon readiness.");
                    }
                    else
                    {
                        ImGuiEx.Text($"loading home.png");
                    }
                }
            });
        }

        Position = new(ImGuiHelpers.MainViewport.Size.X / 2 - ImGui.GetWindowSize().X / 2, 20);
    }
}
