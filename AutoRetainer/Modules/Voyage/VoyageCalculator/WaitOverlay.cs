namespace AutoRetainer.Modules.Voyage.VoyageCalculator
{
    internal class WaitOverlay : Window
    {
        public WaitOverlay() : base("WaitOverlay", ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse, true)
        {
            this.IsOpen = true;
            this.Position = Vector2.Zero;
            this.RespectCloseHotkey = false;
        }

        internal volatile bool IsProcessing = false;
        internal long StartTime = 0;
        internal int Frame = 0;

        public override bool DrawConditions()
        {
            return IsProcessing;
        }

        public override void PreDraw()
        {
            ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
        }

        public override void Draw()
        {
            if (ImGui.GetFrameCount() - Frame > 1) StartTime = Environment.TickCount64;
            Frame = ImGui.GetFrameCount();
            CImGui.igBringWindowToDisplayFront(CImGui.igGetCurrentWindow());
            ImGui.Dummy(new(ImGuiHelpers.MainViewport.Size.X, ImGuiHelpers.MainViewport.Size.Y / 3));
            ImGuiEx.ImGuiLineCentered("Waitoverlay1", () => ImGuiEx.Text($"Calculating optimized path. Please wait."));
            ImGuiEx.ImGuiLineCentered("Waitoverlay2", () => ImGuiEx.Text($"This can take several minutes."));
            ImGuiEx.Text("");
            var span = TimeSpan.FromMilliseconds(Environment.TickCount64 - StartTime);
            ImGuiEx.ImGuiLineCentered("Waitoverlay4", () => ImGuiEx.Text($"{span.Minutes:D2}:{span.Seconds:D2}"));
            ImGuiEx.Text("");
            ImGuiEx.Text("");
            ImGuiEx.ImGuiLineCentered("Waitoverlay3", () =>
            {
                if(ImGui.Button("Hide this overlay"))
                {
                    this.IsProcessing = false;
                }
            });
        }
    }
}
