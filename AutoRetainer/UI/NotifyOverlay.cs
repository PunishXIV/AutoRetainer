using AutoRetainer.Multi;
using Dalamud.Game.ClientState.Conditions;

namespace AutoRetainer.UI;

internal class NotifyOverlay : Window
{
    public NotifyOverlay() : base("AutoRetainer Notify", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize, true)
    {
        this.IsOpen = true;
        this.ShowCloseButton = false;
        this.Position = Vector2.Zero;
        this.RespectCloseHotkey = false;
    }

    public override void Draw()
    {
        CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());
        this.SizeConstraints = new()
        {
            MaximumSize = ImGuiHelpers.MainViewport.Size,
            MinimumSize = new(ImGuiHelpers.MainViewport.Size.X, 0)
        };
        ImGui.SetWindowFontScale(2f);
        ImGui.PushStyleColor(ImGuiCol.Text, GradientColor.Get(ImGuiColors.ParsedGreen, ImGuiColors.DalamudWhite, 1000));
        ImGuiEx.ImGuiLineCentered("notify", delegate
        {
            ImGuiEx.Text($"Retainers available for ventures.");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    NotificationHandler.IsHidden = true;
                    Svc.Commands.ProcessCommand("/ays");
                }
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    NotificationHandler.IsHidden = true;
                }
                ImGui.SetTooltip("Left click - open AutoRetainer;\nRight click - dismiss.");
            }
        });

        ImGui.PopStyleColor();
    }

    public override bool DrawConditions()
    {
        return P.config.NotifyEnableOverlay && NotificationHandler.CurrentState && !NotificationHandler.IsHidden && (!P.config.NotifyCombatDutyNoDisplay || !(Svc.Condition[ConditionFlag.BoundByDuty56] && Svc.Condition[ConditionFlag.InCombat]));
    }
}
