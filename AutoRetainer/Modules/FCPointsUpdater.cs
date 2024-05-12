using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.Events;
using ECommons.EzEventManager;
using ECommons.GameHelpers;
using ECommons.Singletons;
using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules;
public sealed unsafe class FCPointsUpdater
{
		private readonly TaskManager TaskManager = new(new(timeLimitMS: 15000, abortOnTimeout: true, showDebug:true));
		private int OldFCPoints;

		private FCPointsUpdater()
		{
				ProperOnLogin.RegisterInteractable(() => ScheduleUpdateIfNeeded(), true);
				new EzLogout(() => TaskManager.Abort());
				new EzTerritoryChanged((x) => ScheduleUpdateIfNeeded());
		}

		public bool IsFCChestReady()
		{
				if(TryGetAddonByName<AtkUnitBase>("FreeCompanyChest", out var addon) && IsAddonReady(addon))
				{
						var reader = new ReaderFreeCompanyChest(addon);
						return reader.Ready;
				}
				return false;
		}

		public class ReaderFreeCompanyChest(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
		{
				public bool Ready => ReadUInt(10) == 0;
		}

		public void ScheduleUpdateIfNeeded(bool force = false)
		{
				if (!Player.Available) return;
				if (!C.UpdateStaleFCData) return;
				if (!Player.IsInHomeWorld) return;
				if(Data != null && Data.FCID != 0 && C.FCData.TryGetValue(Data.FCID, out var fcdata))
				{
						if(force || DateTimeOffset.Now.ToUnixTimeMilliseconds() > fcdata.FCPointsLastUpdate + 30 * 60 * 60 * 1000)
						{
								OldFCPoints = Utils.FCPoints;
								TaskManager.Abort();
								TaskManager.Enqueue(IsScreenReady);
								TaskManager.Enqueue(() =>
								{
										if (TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon))
										{
												addon->Close(true);
												TaskManager.InsertDelay(10, true);
										}
								});
								TaskManager.Enqueue(() => Chat.Instance.ExecuteCommand("/freecompanycmd"));
								/*TaskManager.Enqueue(() =>
								{
										if(TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon))
										{
												if (addon->IsVisible)
												{
														addon->IsVisible = false;
														return true;
												}
										}
										return false;
								});*/
								TaskManager.Enqueue(() =>
								{
										if (TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon))
										{
												addon->Close(true);
												return true;
										}
										return false;
								});
								TaskManager.Enqueue(() => Utils.FCPoints != OldFCPoints, new(abortOnTimeout: false));
								TaskManager.Enqueue(() => OfflineDataManager.WriteOfflineData(false, true));
						}
				}
		}
}
