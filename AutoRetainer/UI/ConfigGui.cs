using Dalamud.Interface.Components;
using ECommons.Configuration;
using PunishLib.ImGuiMethods;
using AutoRetainer.UI.Settings;
using Dalamud.Interface.Style;

namespace AutoRetainer.UI;

unsafe internal class ConfigGui : Window
{
    public ConfigGui() : base($"{P.Name} {P.GetType().Assembly.GetName().Version}###AutoRetainer")
    {
        this.SizeConstraints = new()
        {
            MinimumSize = new(250, 100),
            MaximumSize = new(9999,9999)
        };
        P.ws.AddWindow(this);
    }

    public override void PreDraw()
    {
        if (!P.config.NoTheme)
        {
            P.Style.Push();
            P.StylePushed = true;
        }
    }

    public override void Draw()
    {
        var e = SchedulerMain.PluginEnabledInternal;
        var disabled = MultiMode.Active && !ImGui.GetIO().KeyCtrl;
        if (disabled)
        {
            ImGui.BeginDisabled();
        }
        if (ImGui.Checkbox($"Enable {P.Name} (automatic mode)", ref e))
        {
            P.WasEnabled = false;
            if(e)
            {
                SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
            }
            else
            {
                SchedulerMain.DisablePlugin();
            }
        }
        if (disabled)
        {
            ImGui.EndDisabled();
            ImGuiComponents.HelpMarker($"MultiMode controls this option. Hold CTRL to override.");
        }

        if (P.WasEnabled)
        {
            ImGui.SameLine();
            ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudGrey, ImGuiColors.DalamudGrey3, 500), $"Paused");
        }
        ImGui.SameLine();
        if(ImGui.Checkbox("Multi", ref MultiMode.Enabled))
        {
            MultiMode.OnMultiModeEnabled();
        }
        if(P.config.CharEqualize && MultiMode.Enabled)
        {
            ImGui.SameLine();
            if(ImGui.Button("Reset counters"))
            {
                MultiMode.CharaCnt.Clear();
            }
        }

        if(IPC.Suppressed)
        {
            ImGuiEx.Text(ImGuiColors.DalamudRed, $"Plugin operation is suppressed by other plugin.");
            ImGui.SameLine();
            if (ImGui.SmallButton("Cancel"))
            {
                IPC.Suppressed = false;
            }
        }

        if (P.TaskManager.IsBusy)
        {
            ImGui.SameLine();
            if (ImGui.Button($"Abort {P.TaskManager.NumQueuedTasks} tasks"))
            {
                P.TaskManager.Abort();
            }
        }


        ImGuiEx.EzTabBar("tabbar",
                ("Retainers", MultiModeUI.Draw, null, true),
                (P.config.RecordStats ? "Statistics" : null, StatisticsUI.Draw, null, true),
                ("Settings", SettingsMain.Draw, null, true),
                (P.config.Expert?"Expert":null, Expert.Draw, null, true),
                //("Beta", Beta.Draw, null, true),
                ("About", delegate { AboutTab.Draw(P); }, null, true),
                (P.config.Verbose ? "Dev" : null, delegate
                {
                    ImGuiEx.EzTabBar("DebugBar",
                        ("Log", InternalLog.PrintImgui, null, false),
                        ("Retainers (old)", Retainers.Draw, null, true),
                        ("Debug", Debug.Draw, null, true),
                        ("WIP", SuperSecret.Draw, null, true)
                    );
                }, null, true)
                );
    }

    public override void PostDraw()
    {
        if (P.StylePushed)
        {
            P.Style.Pop();
            P.StylePushed = false; 
        }
    }

    public override void OnClose()
    {
        EzConfig.Save();
        StatisticsUI.Data.Clear();
        MultiModeUI.JustRelogged = false;
    }

    public override void OnOpen()
    {
        MultiModeUI.JustRelogged = true;
    }
}
