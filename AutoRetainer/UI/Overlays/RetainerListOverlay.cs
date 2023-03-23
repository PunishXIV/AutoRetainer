using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.Overlays;

internal unsafe class RetainerListOverlay : Window
{
    float height;

    public RetainerListOverlay() : base("AutoRetainer retainerlist overlay", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize, true)
    {
        RespectCloseHotkey = false;
        IsOpen = true;
    }

    public override bool DrawConditions()
    {
        if (!P.config.UIBar) return false;
        if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedSummoningBell] && TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && IsAddonReady(addon))
        {
            Position = new(addon->X, addon->Y - height);
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
        var disabled = MultiMode.Enabled && !ImGui.GetIO().KeyCtrl;
        if (disabled)
        {
            ImGui.BeginDisabled();
        }
        if (ImGui.Checkbox("Enable AutoRetainer", ref e))
        {
            P.WasEnabled = false;
            if (e)
            {
                SchedulerMain.EnablePlugin(PluginEnableReason.Manual);
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
        if (P.config.MultiModeUIBar)
        {
            if (ImGui.Checkbox("MultiMode", ref MultiMode.Enabled))
            {
                if (MultiMode.Enabled)
                {
                    SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode);
                }
            }
            ImGui.SameLine();
        }
        if (ImGuiEx.IconButton("\uf013##Open plugin interface"))
        {
            Svc.Commands.ProcessCommand("/ays");
        }
        ImGuiEx.Tooltip("Open plugin configuration window");
        if (!P.TaskManager.IsBusy)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton("\uf24d##Entrust all duplicates"))
            {
                for (var i = 0; i < P.retainerManager.Count; i++)
                {
                    var ret = P.retainerManager.Retainer(i);
                    if (ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskEntrustDuplicates.Enqueue();
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                    }
                }
            }
            ImGuiEx.Tooltip("Entrust duplicates to all retainers");

            ImGui.SameLine();
            if (ImGuiEx.IconButton("\uf51e##WithdrawGil"))
            {
                for (var i = 0; i < P.retainerManager.Count; i++)
                {
                    var ret = P.retainerManager.Retainer(i);
                    if (ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskWithdrawGil.Enqueue(100);
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                    }
                }
            }
            ImGuiEx.Tooltip("Withdraw gil from all retainers");
        }
        height = ImGui.GetWindowSize().Y;
    }

    public override void PostDraw()
    {
        //ImGui.PopStyleVar();
    }
}
