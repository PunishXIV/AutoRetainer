using AutoRetainer.Multi;
using AutoRetainer.NewScheduler;
using AutoRetainer.Statistics;
using ECommons.Configuration;
using ECommons.Reflection;
using PunishLib.ImGuiMethods;
using System.Reflection;

namespace AutoRetainer.UI;

unsafe internal class ConfigGui : Window
{
    public ConfigGui() : base($"{P.Name} {P.GetType().Assembly.GetName().Version}")
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
        if (ImGui.Checkbox($"Enable {P.Name}", ref SchedulerMain.Enabled))
        {
            
        }
        ImGui.SameLine();
        ImGui.Checkbox("Auto Enable", ref P.config.AutoEnableDisable);

        ImGui.SameLine();
        ImGui.Checkbox("Multi", ref MultiMode.Enabled);

        ImGuiEx.EzTabBar("tabbar",

                ("Retainers", MultiModeUI.Draw, null, true),
                (P.config.RecordStats ? "Statistics" : null, StatisticsUI.Draw, null, true),
                ("Settings", Settings.Draw, null, true),
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
        if (P.config.DisableOnClose) 
        {
            SchedulerMain.Enabled = false;
            StatisticsUI.Data.Clear();
            Notify.Success("Auto Retainer disabled");
        }
    }

}
