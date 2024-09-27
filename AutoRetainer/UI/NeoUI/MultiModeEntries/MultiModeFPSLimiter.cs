namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeFPSLimiter : NeoUIEntry
{
    public override string Path => "Multi Mode/FPS Limiter";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("FPS Limiter")
        .TextWrapped("FPS Limiter is only active when Multi Mode is enabled")
        .Widget("Target frame rate when idling", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS(x, ref C.TargetMSPTIdle, C.ExtraFPSLockRange ? 1 : 10);
        })
        .Widget("Target frame rate when idling", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS("Target frame rate when operating", ref C.TargetMSPTRunning, C.ExtraFPSLockRange ? 1 : 20);
        })
        .Checkbox("Release FPS lock when game is active", () => ref C.NoFPSLockWhenActive)
        .Checkbox($"Allow extra low FPS limiter values", () => ref C.ExtraFPSLockRange, "No support is provided if you enable this and run into ANY errors in Multi Mode")
        .Checkbox($"Limiter active only when shutdown timer is set", () => ref C.FpsLockOnlyShutdownTimer);
}
