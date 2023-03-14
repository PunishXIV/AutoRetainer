using AutoRetainer.Multi;
using AutoRetainer.NewScheduler;
using AutoRetainer.Serializables;
using AutoRetainer.Statistics;
using Dalamud.Interface.Components;
using ECommons.Configuration;
using ECommons.Reflection;
using PunishLib.ImGuiMethods;
using System.Reflection;

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
        var e = SchedulerMain.PluginEnabled;
        var disabled = MultiMode.Enabled && !ImGui.GetIO().KeyCtrl;
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
        ImGui.Checkbox("Multi", ref MultiMode.Enabled);

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
                ("Settings", Settings.Draw, null, true),
                (P.config.Expert?"Expert":null, Expert.Draw, null, true),
                ("Beta", TabBeta.Draw, null, true),
                ("About", delegate { AboutTab.Draw(P); }, null, true),
                (P.config.Verbose ? "Log" : null, InternalLog.PrintImgui, null, false),
                (P.config.Verbose?"Retainers (old)":null, Retainers.Draw, null, true),
                (P.config.Verbose?"Debug":null, Debug.Draw, null, true)
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
