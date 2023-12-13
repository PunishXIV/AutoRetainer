using Dalamud.Interface.Components;
using ECommons.Configuration;
using PunishLib.ImGuiMethods;
using AutoRetainer.UI.Settings;
using AutoRetainerAPI.Configuration;
using AutoRetainerAPI;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.UI.Settings.SettingsMain;
using ECommons.Events;

namespace AutoRetainer.UI;

unsafe internal class ConfigGui : Window
{
    public ConfigGui() : base($"")
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
        if (!C.NoTheme)
        {
            P.Style.Push();
            P.StylePushed = true;
        }
        var prefix = SchedulerMain.PluginEnabled ? $" [{SchedulerMain.Reason}]" : "";
        var tokenRem = TimeSpan.FromMilliseconds(P.TimeLaunched + 3*24*60*60*1000 - DateTimeOffset.Now.ToUnixTimeMilliseconds());
        this.WindowName = $"{P.Name} {P.GetType().Assembly.GetName().Version}{prefix} | {FormatToken(tokenRem)}###AutoRetainer";
    }

    string FormatToken(TimeSpan time)
    {
        if (time.TotalMilliseconds > 0)
        {
            if (time.Days > 0)
            {
                return $"Session expires in {time.Days} day{(time.Days == 1 ? "" : "s")}" + (time.Hours>0?$" {time.Hours} hours":"");
            }
            else
            {
                if (time.Hours > 0)
                {
                    return $"Session expires in {time.Hours} hours";
                }
                else
                {
                    return $"Session expires in less than an hour";
                }
            }
        }
        else
        {
            return "Session expired";
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
        if (ImGui.Checkbox($"Enable {P.Name}", ref e))
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
        if (C.ShowDeployables && (VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) || VoyageScheduler.Enabled))
        {
            ImGui.SameLine();
            ImGui.Checkbox($"Deployables", ref VoyageScheduler.Enabled);
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
        if (ImGui.Checkbox("Multi", ref MultiMode.Enabled))
        {
            MultiMode.OnMultiModeEnabled();
        }
        if (C.ShowNightMode)
        {
            ImGui.SameLine();
            if (ImGui.Checkbox("Night", ref P.NightMode))
            {
                MultiMode.BailoutNightMode();
            }
        }
        if (C.DisplayMMType)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.EnumCombo("##mode", ref C.MultiModeType);
        }
        if (C.CharEqualize && MultiMode.Enabled)
        {
            ImGui.SameLine();
            if(ImGui.Button("Reset counters"))
            {
                MultiMode.CharaCnt.Clear();
            }
        }

        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnMainControlsDraw).SendMessage();

        if (IPC.Suppressed)
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
                ("Deployables", WorkshopUI.Draw, null, true),
                (C.RecordStats ? "Statistics" : null, StatisticsUI.Draw, null, true),
                ("Settings", SettingsMain.Draw, null, true),
                (C.Expert?"Expert":null, Expert.Draw, null, true),
                //("Beta", Beta.Draw, null, true),
                ("About", delegate { AboutTab.Draw(P.Name); }, null, true),
                (C.Verbose ? "Dev" : null, delegate
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
