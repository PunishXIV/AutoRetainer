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
        .Checkbox($"Do not reset Preferred Character on Manual Login", () => ref C.MultiNoPreferredReset)
        .Checkbox("Enable Manual relogs character postprocess", () => ref C.AllowManualPostprocess)

        .Section("Game startup")
        .Checkbox($"Enable Multi Mode on Game Boot", () => ref C.MultiAutoStart)
        .Widget("Auto-login on Game Boot", (x) =>
        {
            ImGui.SetNextItemWidth(150f);
            var names = C.OfflineData.Where(s => !s.Name.IsNullOrEmpty()).Select(s => $"{s.Name}@{s.World}");
            var dict = names.ToDictionary(s => s, s => Censor.Character(s));
            dict.Add("", "Disabled");
            ImGuiEx.Combo(x, ref C.AutoLogin, ["", ..names], names: dict);
        })
        .SliderInt(150f, "Delay", () => ref C.AutoLoginDelay.ValidateRange(0, 60), 0, 20, "Set appropriate delay to let plugins fully load before logging in and to allow yourself some time to cancel login if needed")

        .Section("Inventory warnings")
        .InputInt(100f, $"Retainer list: remaining inventory slots warning", () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Retainer list: remaining ventures warning", () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Deployables list: remaining inventory slots warning", () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Deployables list: remaining fuel warning", () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, $"Deployables list: remaining repair kit warning", () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))

        .Section("Teleportation")
        .Widget(() => ImGuiEx.Text("Lifestream plugin is required"))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.1.1"))]))
        .Widget(() => ImGuiEx.TextWrapped("You must register houses in Lifestream plugin for every character you want this option to work or enable Simple Teleport."))
        .Checkbox("Enable teleport to private house", () => ref C.AllowPrivateTeleport)
        .Checkbox("Enable teleport to free company house", () => ref C.AllowFcTeleport)
        .Checkbox("If no private/FC house registered, teleport to apartment or inn", () => ref C.AllowRetireInnApartment)
        .Indent()
        .Checkbox("Disable Apartments", () => ref C.DisableApartment)
        .Unindent()
        .Checkbox("Enable Simple Teleport", () => ref C.AllowSimpleTeleport)
        .Widget(() => ImGuiEx.HelpMarker("Allows teleporting to houses without registering them in Lifestream. You still need Lifestream plugin for teleportation to work.\n\nWarning! This option is less reliable than registering your houses in Lifestream. Avoid it if you can.", EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("Bailout Module")
        .Checkbox("Auto-close and retry logging in on connection errors", () => ref C.ResolveConnectionErrors)
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}
