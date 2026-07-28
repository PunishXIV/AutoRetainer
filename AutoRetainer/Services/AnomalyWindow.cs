using ECommons.SimpleGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.Services;

public class AnomalyWindow : Window
{
    List<Anomaly> Anomalieis = [];
    public AnomalyWindow() : base("AutoRetainer has detected anomalies")
    {
        this.SetSizeConstraints(new(300, 200), new(float.MaxValue));
        this.RespectCloseHotkey = false;
        P.WindowSystem.AddWindow(this);
    }

    public override void Draw()
    {
        if(ImGuiEx.BeginDefaultTable("Anomanies", ["Date", "~Description", "Character"]))
        {
            foreach(var x in Anomalieis)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.Text($"{x.Date.ToLocalTime()}");
                ImGui.TableNextColumn();
                ImGuiEx.TextWrapped($"{x.Description}");
                ImGui.TableNextColumn();
                ImGuiEx.Text($"{Censor.Character(x.Character)}");
            }
            ImGui.EndTable();
        }
    }

    public void Add(string description)
    {
        this.Anomalieis.Add(new(description));
        this.IsOpen = true;
    }

    public override void OnClose()
    {
        Anomalieis.Clear();
    }
}

public class Anomaly
{
    public DateTimeOffset Date = DateTimeOffset.Now;
    public string Description;
    public string Character = Player.NameWithWorld;

    public Anomaly(string description)
    {
        Description = description;
    }
}