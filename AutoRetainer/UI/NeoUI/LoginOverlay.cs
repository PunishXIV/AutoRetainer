namespace AutoRetainer.UI.NeoUI;
public class LoginOverlay : NeoUIEntry
{
    public override string Path => "Login Overlay";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
            .Section("Login Overlay")
            .Checkbox("Display Login Overlay", () => ref C.LoginOverlay)
            .Widget("Login overlay scale multiplier", (x) =>
            {
                ImGuiEx.SetNextItemWidthScaled(150f);
                if(ImGuiEx.SliderFloat(x, ref C.LoginOverlayScale.ValidateRange(0.1f, 5f), 0.2f, 2f)) P.LoginOverlay.bWidth = 0;
            })
            .Widget($"Login overlay button padding", (x) =>
            {
                ImGuiEx.SetNextItemWidthScaled(150f);
                if(ImGuiEx.SliderFloat(x, ref C.LoginOverlayBPadding.ValidateRange(0.5f, 5f), 1f, 1.5f)) P.LoginOverlay.bWidth = 0;
            })
        .Checkbox("Display hidden characters when searching", () => ref C.LoginOverlayAllSearch)
        .SliderInt(150f, "Number of columns", () => ref C.NumLoginOverlayCols.ValidateRange(1, 10), 1, 10)
        .SliderFloat(150f, "Overlay height, %", () => ref C.LoginOverlayPercent.ValidateRange(20f, 100f), 20f, 100f);
}
