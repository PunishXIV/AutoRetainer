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
        var disabled = MultiMode.Active && !ImGui.GetIO().KeyCtrl;
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
                if (MultiMode.Active)
                {
                    SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode);
                }
            }
            ImGui.SameLine();
        }
        if (ImGuiEx.IconButton($"{Lang.IconSettings}##Open plugin interface"))
        {
            Svc.Commands.ProcessCommand("/ays");
        }
        ImGuiEx.Tooltip("Open Plugin Settings");
        if (!P.TaskManager.IsBusy)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton($"{Lang.IconDuplicate}##Entrust all duplicates"))
            {
                for (var i = 0; i < P.retainerManager.Count; i++)
                {
                    var ret = P.retainerManager.Retainer(i);
                    if (ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskEntrustDuplicates.Enqueue();

                        if (P.config.RetainerMenuDelay > 0)
                        {
                            TaskWaitSelectString.Enqueue(P.config.RetainerMenuDelay);
                        }
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                    }
                }
            }
            ImGuiEx.Tooltip("Quick Entrust Duplicates");

            ImGui.SameLine();
            if (ImGuiEx.IconButton($"{Lang.IconGil}##WithdrawGil"))
            {
                for (var i = 0; i < P.retainerManager.Count; i++)
                {
                    var ret = P.retainerManager.Retainer(i);
                    if (ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskWithdrawGil.Enqueue(100);

                        if (P.config.RetainerMenuDelay > 0)
                        {
                            TaskWaitSelectString.Enqueue(P.config.RetainerMenuDelay);
                        }
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                    }
                }
            }
            ImGuiEx.Tooltip("Quick Withdraw Gil");
        }
        height = ImGui.GetWindowSize().Y;
    }

    public override void PostDraw()
    {
        //ImGui.PopStyleVar();
    }
}
