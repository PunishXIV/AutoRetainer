using AutoRetainer.Multi;

namespace AutoRetainer.UI;

internal class MultiModeOverlay : Window
{
    public MultiModeOverlay() : base("AutoRetainer Multi Mode Warning", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |  ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize, true)
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
        ImGui.SetWindowFontScale(3f);
        ImGui.PushStyleColor(ImGuiCol.Text, GradientColor.Get(ImGuiColors.DalamudYellow, ImGuiColors.DalamudRed, 1000));
        ImGuiEx.ImGuiLineCentered("multi", delegate
        {
            if (MultiMode.Enabled)
            {
                ImGuiEx.Text($"AutoRetainer Multi Mode Active. Click to disable.");
            }
            else
            {
                ImGuiEx.Text($"Automatically relogging, click to abort.");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    if (MultiMode.Enabled)
                    {
                        MultiMode.Enabled = false;
                    }
                    else
                    {
                        AutoLogin.Instance.Abort();
                    }
                }
            }
        });
        
        ImGui.PopStyleColor();
    }

    public override bool DrawConditions()
    {
        return MultiMode.Enabled || AutoLogin.Instance.IsRunning;
    }
}
