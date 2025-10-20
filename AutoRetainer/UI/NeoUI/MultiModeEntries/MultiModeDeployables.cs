namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
    public override string Path => "Multi Mode/Deployables";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Multi Mode - Deployables")
        .Checkbox("Wait For Voyage Completion", () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, """When enabled, AutoRetainer will wait for all deployables to return before logging into the character. If you're already logged in for another reason, it will still resend completed submarines—unless the global setting "Wait even when already logged in" is also turned on.""")
        .Indent()
        .Checkbox("Wait even when already logged in", () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn, """Changes the behavior of "Wait for Voyage Completion" (both global and per-character) so that AutoRetainer no longer resends individual submarines while already logged in. Instead, it will wait until all submarines have returned before taking action.""")
        .InputInt(120f, "Maximum Wait, minutes", () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, """If waiting for other deployables to return would exceed this number of minutes, AutoRetainer will ignore both the "Wait for Voyage Completion" and "Wait even when already logged in" settings.""")
        .Unindent()
        .DragInt(60f, "Advance Relog Threshold, seconds", () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300, "The number of seconds AutoRetainer should log in early before submarines on this character are ready to be resent.")
        .DragInt(120f, "Retainer venture processing cutoff, minutes", () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "If set to a value greater than 0, AutoRetainer will stop processing any retainers this number of minutes before any character is scheduled to redeploy submarines, taking all previous settings into account.")
        .Checkbox("Sell items from Unconditional sell list right after deployment (requires retainers)", () => ref C.VendorItemAfterVoyage)
        .Checkbox("Periodically check FC chest for gil upon entering workshop", () => ref C.FCChestGilCheck, "Periodically checks the Free Company chest when entering the Workshop to keep the gil counter up to date.")
        .Indent()
        .SliderInt(150f, "Check frequency, hours", () => ref C.FCChestGilCheckCd, 0, 24 * 5)
        .Widget("Reset cooldowns", (x) =>
        {
            if(ImGuiEx.Button(x, C.FCChestGilCheckTimes.Count > 0)) C.FCChestGilCheckTimes.Clear();
        })
        .Unindent();
}
