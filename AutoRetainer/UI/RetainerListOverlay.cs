using AutoRetainer.GcHandin;
using AutoRetainer.Multi;
using AutoRetainer.NewScheduler;
using AutoRetainer.NewScheduler.Handlers;
using AutoRetainer.NewScheduler.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutoRetainer.UI
{
    internal unsafe class RetainerListOverlay : Window
    {
        float height;

        public RetainerListOverlay() : base("AutoRetainer retainerlist overlay", ImGuiWindowFlags.NoDecoration |  ImGuiWindowFlags.AlwaysAutoResize, true)
        {
            this.RespectCloseHotkey = false;
            this.IsOpen = true;
        }

        public override bool DrawConditions()
        {
            if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedSummoningBell] && TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && IsAddonReady(addon))
            {
                this.Position = new(addon->X, addon->Y - height);
                return true;
            }
            return false;
        }

        public override void PreDraw()
        {
            //ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        public override void Draw()
        {
            var e = SchedulerMain.PluginEnabled;
            if(ImGui.Checkbox("Enable AutoRetainer", ref e))
            {
                if (e)
                {
                    SchedulerMain.EnablePlugin(Serializables.PluginEnableReason.Manual);
                }
                else
                {
                    SchedulerMain.DisablePlugin();
                }
            }
            ImGui.SameLine();
            if(ImGui.Checkbox("MultiMode", ref MultiMode.Enabled))
            {
                if (MultiMode.Enabled)
                {
                    SchedulerMain.EnablePlugin(Serializables.PluginEnableReason.MultiMode);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Open plugin interface"))
            {
                Svc.Commands.ProcessCommand("/ays");
            }
            if (!P.TaskManager.IsBusy)
            {
                ImGui.SameLine();
                if (ImGui.Button("Entrust all duplicates"))
                {
                    for (var i = 0; i < P.retainerManager.Count; i++)
                    {
                        var ret = P.retainerManager.Retainer(i);
                        if(ret.Available)
                        {
                            P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                            TaskEntrustDuplicates.Enqueue();
                            P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                        }
                    }
                }
            }
            height = ImGui.GetWindowSize().Y;
        }

        public override void PostDraw()
        {
            //ImGui.PopStyleVar();
        }
    }
}
