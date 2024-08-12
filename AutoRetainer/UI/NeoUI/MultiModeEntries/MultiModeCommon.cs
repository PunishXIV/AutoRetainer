using NightmareUI.PrimaryUI;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "Multi Mode/Common Settings";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Common Settings")
        .Checkbox($"Enforce Full Character Rotation", () => ref C.CharEqualize, "Recommended for users with > 15 characters, forces multi mode to make sure ventures are processed on all characters in order before returning to the beginning of the cycle.")
        .Checkbox($"Wait on login screen", () => ref C.MultiWaitOnLoginScreen, "If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.")
        .Checkbox("Synchronise Retainers (one time)", () => ref MultiMode.Synchronize, "AutoRetainer will wait until all enabled retainers have completed their ventures. After that this setting will be disabled automatically and all characters will be processed.")
        .Checkbox($"Disable Multi Mode on Manual Login", () => ref C.MultiDisableOnRelog)
        .Checkbox($"Enable Multi Mode on Game Boot", () => ref C.MultiAutoStart)
        .Checkbox($"Do not reset Preferred Character on Manual Login", () => ref C.MultiNoPreferredReset)

        .Section("Inventory warnings")
        .InputInt(100f, $"Retainer list: remaining inventory slots warning", () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Retainer list: remaining ventures warning", () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Deployables list: remaining inventory slots warning", () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Deployables list: remaining fuel warning", () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, $"Deployables list: remaining repair kit warning", () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))

        .Section("Teleportatiopn")
        .Widget(() => ImGuiEx.Text("Lifestream plugin is required"))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.0.4"))]))
        .Widget(() => ImGuiEx.TextWrapped("You must register houses in Lifestream plugin for every character you want this option to work."))
        .Checkbox("Enable teleport to private house", () => ref C.AllowPrivateTeleport)
        .Checkbox("Enable teleport to free company house", () => ref C.AllowFcTeleport);
}
