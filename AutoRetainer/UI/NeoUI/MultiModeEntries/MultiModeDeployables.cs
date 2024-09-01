namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
    public override string Path => "Multi Mode/Deployables";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
            .Section("Multi Mode - Deployables")
            .Checkbox("Wait For Voyage Completion", () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, "AutoRetainer will wait for all deployables to return before cycling to the next character in multi mode operation.")
            .InputInt(120f, "Maximum Wait, minutes", () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, "If waiting for other deployables to return would exceed this amount of minutes, AutoRetainer will ignore the setting.")
            .DragInt(60f, "Advance Relog Threshold", () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
            .Checkbox("Wait even when already logged in", () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)
            .DragInt(120f, "Retainer venture processing cutoff", () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "The number of minutes remaining on deployable voyages to prevent processing of retainer tasks.");
}
