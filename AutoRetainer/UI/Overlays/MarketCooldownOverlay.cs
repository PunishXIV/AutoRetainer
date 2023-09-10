using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.Overlays
{
    internal class MarketCooldownOverlay : Window
    {
        public long UnlockAt = 0;

        public MarketCooldownOverlay() : base("AutoRetainer MarketCooldownOverlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.IsOpen = true;
            this.RespectCloseHotkey = false;
        }

        public override void PreDraw()
        {
            this.SizeConstraints = new()
            {
                MinimumSize = new(ImGuiHelpers.MainViewport.Size.X, 0),
                MaximumSize = new(0, float.MaxValue)
            };
        }

        public override void Draw()
        {
            CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());
            var percent = 1f - (float)(UnlockAt - Environment.TickCount64) / 2000f;
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, EColor.Green);
            ImGui.ProgressBar(percent, new(ImGui.GetContentRegionAvail().X, 20), $"");
            ImGui.PopStyleColor();
            this.Position = new(0, 0);
        }

        public override bool DrawConditions()
        {
            return Environment.TickCount64 < UnlockAt;
        }
    }
}
