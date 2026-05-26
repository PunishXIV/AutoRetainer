using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;

public class MultiModeDisableRender : NeoUIEntry
{
    public override string Path => "Multi Mode/Disable Render";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("Disable Render")
        .Checkbox("Disable Render when in Multi Mode", () => ref C.MultiDisableRender, "Disables world rendering while in Multi Mode.")
        .Checkbox("Only when in Night Mode", () => ref C.MultiDisableRenderNightModeOnly)
        .Checkbox("Only when window is not active", () => ref C.MultiDisableRenderOnlyInactive);
}
