using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Handlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Scheduler.Tasks;

internal static unsafe class TaskEntrustDuplicates
{
		internal static bool NoDuplicates = false;

		internal static unsafe bool CheckNoDuplicates()
		{
				for (var rI = InventoryType.RetainerPage1; rI <= InventoryType.RetainerPage7; rI++)
				{
						var inv = FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryContainer(rI);
						if (inv == null || inv->Loaded == 0) continue;
						for (var slot = 0; slot < inv->Size; slot++)
						{
								var slotItem = inv->GetInventorySlot(slot);
								if (slotItem == null) continue;
								if (FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(slotItem->ItemId, slotItem->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality)) > 0)
								{
										if (!Data.TransferItemsBlacklist.Contains(slotItem->ItemId))
										{
												return false;
										}
								}
						}
				}
				return true;
		}

		internal static void Enqueue()
		{
				P.TaskManager.Enqueue(() => { NoDuplicates = CheckNoDuplicates(); return true; });
				P.TaskManager.Enqueue(() => { NoDuplicates = false; return true; });
				P.TaskManager.Enqueue(NewYesAlreadyManager.WaitForYesAlreadyDisabledTask);
				if (C.RetainerMenuDelay > 0)
				{
						TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
				}
				P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.SelectEntrustItems(); });
				P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickEntrustDuplicates(); });
				TaskWait.Enqueue(500);
				P.TaskManager.Enqueue(UncheckBlacklistedItems);
				TaskWait.Enqueue(500);
				P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickEntrustDuplicatesConfirm(); }, 600 * 1000, false);
				TaskWait.Enqueue(500);
				P.TaskManager.Enqueue(() => { if (NoDuplicates) return true; return RetainerHandlers.ClickCloseEntrustWindow(); }, false);
				P.TaskManager.Enqueue(RetainerHandlers.CloseAgentRetainer);
		}

		internal static bool? UncheckBlacklistedItems()
		{
				if (NoDuplicates) return true;
				if (TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var addon) && IsAddonReady(addon))
				{
						if (Utils.GenericThrottle)
						{
								var reader = new ReaderRetainerItemTransferList(addon);
								var cnt = 0;
								for (int i = 0; i < reader.Items.Count; i++)
								{
										if (Data.TransferItemsBlacklist.Contains(reader.Items[i].ItemID))
										{
												cnt++;
												PluginLog.Debug($"Removing item {reader.Items[i].ItemID} at position {i} as it was in blacklist");
												Callback.Fire(addon, true, 0, (uint)i);
										}
								}
								if (cnt == reader.Items.Count)
								{
										NoDuplicates = true;
										addon->Close(true);
								}
								return true;
						}
				}
				else
				{
						Utils.RethrottleGeneric();
				}
				return false;
		}
}
