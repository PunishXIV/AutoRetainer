namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "Multi Mode/Common Settings";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Common Settings")
        .Checkbox($"Wait on login screen", () => ref C.MultiWaitOnLoginScreen, "If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.")
        .Checkbox($"Disable Multi Mode on Manual Login", () => ref C.MultiDisableOnRelog, "Upon relogging via AutoRetainer's UI or command, disable Multi Mode.")
        .Checkbox($"Do not reset Preferred Character on Manual Login", () => ref C.MultiNoPreferredReset, "Upon relogging via AutoRetainer's UI or command, do not reset preferred character.")
        .Checkbox("Allow entering shared houses", () => ref C.SharedHET)
        .Checkbox("Attempt to enter house on login even when Multi Mode is disabled", () => ref C.HETWhenDisabled)
        .Checkbox("Do not teleport or enter house for retainers when already next to bell", () => ref C.NoTeleportHetWhenNextToBell)

        .Section("Game startup")
        .Checkbox($"Enable Multi Mode on Game Boot", () => ref C.MultiAutoStart)
        .Widget("Auto-login on Game Boot", (x) =>
        {
            ImGui.SetNextItemWidth(150f);
            var names = C.OfflineData.Where(s => !s.Name.IsNullOrEmpty()).Select(s => $"{s.Name}@{s.World}");
            var dict = names.ToDictionary(s => s, s => Censor.Character(s));
            dict.Add("", "Disabled");
            dict.Add("~", "Last logged in character");
            ImGuiEx.Combo(x, ref C.AutoLogin, ["", "~", .. names], names: dict);
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
        .TextWrapped("You must register houses in Lifestream plugin for every character you want this option to work or enable Simple Teleport.")
        .TextWrapped("You can customize these settings per character in character configuration menu.")
        .Widget(() =>
        {
            if(Data != null && Data.GetAreTeleportSettingsOverriden())
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "For current character teleport options are customized.");
            }
        })
        .Checkbox("Enabled", () => ref C.GlobalTeleportOptions.Enabled)
        .Indent()
        .Checkbox("Teleport for retainers...", () => ref C.GlobalTeleportOptions.Retainers)
        .Indent()
        .Checkbox("...to private house", () => ref C.GlobalTeleportOptions.RetainersPrivate)
        .Checkbox("...to shared estate", () => ref C.GlobalTeleportOptions.RetainersShared)
        .Checkbox("...to free company house", () => ref C.GlobalTeleportOptions.RetainersFC)
        .Checkbox("...to apartment", () => ref C.GlobalTeleportOptions.RetainersApartment)
        .TextWrapped("If all above are disabled or fail, will be teleported to inn.")
        .Unindent()
        .Checkbox("Teleport to free company house for deployables", () => ref C.GlobalTeleportOptions.Deployables)
        .Checkbox("Enable Simple Teleport", () => ref C.AllowSimpleTeleport)
        .Unindent()
        .Widget(() => ImGuiEx.HelpMarker("""
            Allows teleporting to houses without registering them in Lifestream. Note: the Lifestream plugin is still required for teleportation to work.

            Warning: This option is less reliable than registering your houses in Lifestream. Use it only if necessary.
            """, EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("Bailout Module")
        .Checkbox("Auto-close and retry logging in on connection errors", () => ref C.ResolveConnectionErrors, "Upon disconnecting, AutoRetainer will attempt to log back in. If the session has expired, no login attempt will be made.")
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}
