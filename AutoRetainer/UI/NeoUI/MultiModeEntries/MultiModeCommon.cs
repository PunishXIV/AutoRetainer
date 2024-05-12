using NightmareUI.PrimaryUI;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon: NeoUIEntry
{
		public override string Path => "Multi Mode/Common Settings";

		public override NuiBuilder Builder { get; init; } = new NuiBuilder()
				.Section("Common Settings")
						.Checkbox($"Enforce Full Character Rotation", () => ref C.CharEqualize, "Recommended for users with > 15 characters, forces multi mode to make sure ventures are processed on all characters in order before returning to the beginning of the cycle.")
						.Checkbox($"Wait on login screen", () => ref C.MultiWaitOnLoginScreen, "If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.")
						.Checkbox("Synchronise Retainers (one time)", () => ref MultiMode.Synchronize, "AutoRetainer will wait until all enabled retainers have completed their ventures. After that this setting will be disabled automatically and all characters will be processed.")
						.Checkbox($"Disable Multi Mode on Manual Login", () => ref C.MultiDisableOnRelog)
						.Checkbox($"Enable Multi Mode on Game Boot", () => ref C.MultiAutoStart)
						.Checkbox($"Do not reset Preferred Character on Manual Login", () => ref C.MultiNoPreferredReset);

}
