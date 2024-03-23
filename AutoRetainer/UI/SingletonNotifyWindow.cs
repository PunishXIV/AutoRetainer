using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI;
public class SingletonNotifyWindow : NotifyWindow
{
    bool IAmIdiot = false;
    WindowSystem ws;
    public SingletonNotifyWindow() : base("AutoRetainer - warning!")
    {
        this.IsOpen = true;
        ws = new();
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        ws.AddWindow(this);
    }

    public override void OnClose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
    }

    public override void DrawContent()
    {
        ImGuiEx.Text($"AutoRetainer has detected that another instance of the plugin is running \nwith the same data path configuration.");
        ImGuiEx.Text($"Plugin load has been halted in order to prevent data loss.");
        if(ImGui.Button("Close this window without loading AutoRetainer"))
        {
            this.IsOpen = false;
        }
        if(ImGui.Button("Learn how to properly run 2 or more game instances"))
        {
            ShellStart("https://github.com/PunishXIV/AutoRetainer/issues/62");
        }
        ImGui.Separator();
        ImGui.Checkbox($"I agree that I may lose all AutoRetainer data", ref IAmIdiot);
        if (!IAmIdiot) ImGui.BeginDisabled();
        if (ImGui.Button("Load AutoRetainer"))
        {
            this.IsOpen = false;
            new TickScheduler(P.Load);
        }
        if (!IAmIdiot) ImGui.EndDisabled();
    }
}
