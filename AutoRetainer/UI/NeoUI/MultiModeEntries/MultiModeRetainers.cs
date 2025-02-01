namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeRetainers : NeoUIEntry
{
    public override string Path => "Multi Mode/Retainers";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Multi Mode - Retainers")
        .Checkbox("Wait For Venture Completion", () => ref C.MultiModeRetainerConfiguration.MultiWaitForAll, "AutoRetainer will wait for all retainers to return before cycling to the next character in multi mode operation.")
        .DragInt(60f, "Advance Relog Threshold", () => ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
        .SliderInt(100f, "Minimum inventory slots to continue operation", () => ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30)
        .TextWrapped("The following ordering settings will affect order of submersibles even though they only read retainer data")
        .Indent()
        .Checkbox("Order characters by venture completion time", () => ref C.LongestVentureFirst, "Characters that have completed ventures longer time ago will be checked first")
        .Checkbox("Order characters by retainer level and cap", () => ref C.CappedLevelsLast, "Characters with retainers that can be levelled up will be done first; then, characters with retainers at max level; and then characters with retainers less than max level and level capped.")
        .Unindent();
}
