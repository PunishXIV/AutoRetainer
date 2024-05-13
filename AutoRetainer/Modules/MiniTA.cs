using AutoRetainer.Modules.Voyage;
using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AutoRetainer.Modules;

internal static unsafe class MiniTA
{
		internal static void Tick()
		{
				if (!IPC.Suppressed)
				{
						if (VoyageScheduler.Enabled)
						{
								ConfirmCutsceneSkip();
								ConfirmRepair();
						}
						if (P.TaskManager.IsBusy || (Svc.Condition[ConditionFlag.OccupiedSummoningBell] && (SchedulerMain.PluginEnabled || P.TaskManager.IsBusy || P.ConditionWasEnabled)))
						{
								if (TryGetAddonByName<AddonTalk>("Talk", out var addon) && addon->AtkUnitBase.IsVisible)
								{
										ClickTalk.Using((nint)addon).Click();
								}
						}
				}
		}

		internal static void ConfirmRepair()
		{
				var x = Utils.GetSpecificYesno((s) => s.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.WorkshopRepairConfirm));
				if (x != null && Utils.GenericThrottle)
				{
						VoyageUtils.Log("Confirming repair");
						ClickSelectYesNo.Using((nint)x).Yes();
				}
		}

		internal static void ConfirmCutsceneSkip()
		{
				var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
				if (addon == IntPtr.Zero) return;
				var selectStrAddon = (AddonSelectString*)addon;
				if (!IsAddonReady(&selectStrAddon->AtkUnitBase))
				{
						return;
				}
				//PluginLog.Debug($"1: {selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString()}");
				if (!Lang.SkipCutsceneStr.Contains(selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString())) return;
				if (EzThrottler.Throttle("SkipCutsceneConfirm"))
				{
						PluginLog.Debug("Selecting cutscene skipping");
						ClickSelectString.Using(addon).SelectItem(0);
				}
		}

		internal static bool ProcessCutsceneSkip(nint arg)
		{
				return VoyageScheduler.Enabled;
		}
}
