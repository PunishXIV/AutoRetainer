using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.Overlays;

internal unsafe class RetainerListOverlay : Window
{
    float height;
    internal volatile string PluginToProcess = null;

    public RetainerListOverlay() : base("AutoRetainer retainerlist overlay", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize, true)
		{
				P.WindowSystem.AddWindow(this);
				RespectCloseHotkey = false;
        IsOpen = true;
    }

    public override bool DrawConditions()
    {
        if (!C.UIBar) return false;
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
        if (C.MultiModeUIBar)
        {
            ImGui.SameLine();
            if (ImGui.Checkbox("MultiMode", ref MultiMode.Enabled))
            {
                MultiMode.OnMultiModeEnabled();
                if (MultiMode.Active)
                {
                    SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode);
                }
            }
        }

        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnMainControlsDraw).SendMessage();

        ImGui.SameLine();

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
                for (var i = 0; i < GameRetainerManager.Count; i++)
                {
                    var ret = GameRetainerManager.Retainers[i];
                    if (ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskEntrustDuplicates.Enqueue();

                        if (C.RetainerMenuDelay > 0)
                        {
                            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                        }
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                    }
                }
            }
            ImGuiEx.Tooltip("Quick Entrust Duplicates");

            ImGui.SameLine();
            if (ImGuiEx.IconButton($"{Lang.IconGil}##WithdrawGil"))
            {
                for (var i = 0; i < GameRetainerManager.Count; i++)
                {
                    var ret = GameRetainerManager.Retainers[i];
                    if (ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskWithdrawGil.Enqueue(100);

                        if (C.RetainerMenuDelay > 0)
                        {
                            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                        }
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                    }
                }
            }
            ImGuiEx.Tooltip("Quick Withdraw Gil");

            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton($"{Lang.IconFire}##vendoritems"))
                {
                    for (var i = 0; i < GameRetainerManager.Count; i++)
                    {
                        var ret = GameRetainerManager.Retainers[i];
                        if (ret.Available)
                        {
                            P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                            TaskVendorItems.Enqueue();

                            if (C.RetainerMenuDelay > 0)
                            {
                                TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                            }
                            P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                        }
                    }
                }
                ImGuiEx.Tooltip("Quick Vendor Items");
            }

            PluginToProcess = null;
            Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnRetainerListTaskButtonsDraw).SendMessage();
            if(PluginToProcess != null)
            {
                for (var i = 0; i < GameRetainerManager.Count; i++)
                {
                    var ret = GameRetainerManager.Retainers[i];
                    if (ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskPostprocessRetainerIPC.Enqueue(ret.Name.ToString(), PluginToProcess);

                        if (C.RetainerMenuDelay > 0)
                        {
                            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                        }
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                        P.TaskManager.Enqueue(RetainerHandlers.ConfirmCantBuyback);
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
