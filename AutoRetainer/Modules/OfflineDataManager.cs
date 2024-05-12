using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using ECommons.Automation;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Singletons;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Modules;

internal unsafe static class OfflineDataManager
{
    internal static void EnqueueWriteWhenPlayerAvailable()
    {
        P.ODMTaskManager.Abort();
        P.ODMTaskManager.Enqueue(() =>
        {
            if (!Player.Available) return false;
            WriteOfflineData(false, false);
            return true;
        });
    }

    internal static void Tick()
    {
        if (Svc.Condition[ConditionFlag.OccupiedSummoningBell])
        {
            if (GameRetainerManager.Ready)
            {
                WriteOfflineData(false, false);
            }
            if(EzThrottler.Throttle("Periodic.CalculateItemLevel") && Utils.TryGetCurrentRetainer(out var ret))
            {
                var adata = Utils.GetAdditionalData(Player.CID, ret);
                var result = Helpers.ItemLevel.Calculate(out var g, out var p);
                if(result != null)
                {
                    adata.Ilvl = result.Value;
                    adata.Gathering = g;
                    adata.Perception = p;
                }
            }
        }
				if ((MultiMode.Active || AutoGCHandin.Operation || Utils.IsBusy || P.ConfigGui.IsOpen || Svc.Condition[ConditionFlag.LoggingOut] || Svc.Condition[ConditionFlag.OccupiedSummoningBell]) && EzThrottler.Throttle("Periodic.WriteOfflineData", 1000))
				{
						WriteOfflineData(false, EzThrottler.Throttle("Periodic.SaveData", 1000 * 60 * 5));
				}
		}

    internal static void WriteOfflineData(bool writeGatherables, bool saveConfig)
    {
        if (!ProperOnLogin.PlayerPresent) return;
        if (C.Blacklist.Any(x => x.CID == Svc.ClientState.LocalContentId)) return;
        if (!C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            data = new()
            {
                CID = Svc.ClientState.LocalContentId,
            };
            C.OfflineData.Add(data);
        }
        data.World = ExcelWorldHelper.GetName(Svc.ClientState.LocalPlayer.HomeWorld.Id);
        data.Name = Svc.ClientState.LocalPlayer.Name.ToString();
        if(Player.Object.CurrentWorld.GameData.DataCenter.Row != Player.Object.HomeWorld.GameData.DataCenter.Row)
        {
            data.WorldOverride = Player.CurrentWorld;
        }
        else
        {
            data.WorldOverride = null;
        }
        data.Gil = (uint)InventoryManager.Instance()->GetInventoryItemCount(1);
        for (int i = 0; i < 30; i++)
        {
            data.ClassJobLevelArray[i] = UIState.Instance()->PlayerState.ClassJobLevelArray[i];
        }
        if (writeGatherables)
        {
            try
            {
                data.UnlockedGatheringItems.Clear();
                foreach (var x in Svc.Data.GetExcelSheet<GatheringItem>())
                {
                    if (P.Memory.IsGatheringItemGathered(x.RowId))
                    {
                        data.UnlockedGatheringItems.Add(x.RowId);
                    }
                }
            }
            catch (Exception e)
            {
                e.Log();
            }
        }
        if (GameRetainerManager.Ready && GameRetainerManager.Count > 0 && Player.IsInHomeWorld)
        {
            var cleared = false;
            for (var i = 0; i < GameRetainerManager.Count; i++)
            {
                var ret = GameRetainerManager.Retainers[i];
                if (ret.RetainerID == 0) continue;
                if (ret.RetainerID != 0 && !cleared)
                {
                    data.RetainerData.Clear();
                    cleared = true;
                }
                data.RetainerData.Add(new()
                {
                    Name = ret.Name.ToString(),
                    VentureEndsAt = ret.VentureCompleteTimeStamp,
                    HasVenture = ret.VentureID != 0,
                    Level = ret.Level,
                    Job = ret.ClassJob,
                    VentureID = ret.VentureID, 
                    Gil = ret.Gil,
                    RetainerID = ret.RetainerID,
                    MBItems = ret.MarkerItemCount,
                });

                for (int p = 0; p < GameRetainerManager.Count; p++)
                {
                    if (RetainerManager.Instance()->DisplayOrder[p] == i)
                    {
                        data.RetainerData[i].DisplayOrder = p;
                        break;
                    }
                }
            }
        }
        if (Player.IsInHomeWorld)
        {
            var fc = InfoModule.Instance()->GetInfoProxyFreeCompany();
            data.FCID = fc->ID;
            if (!C.FCData.ContainsKey(fc->ID)) C.FCData[fc->ID] = new();
						C.FCData[fc->ID].Name = MemoryHelper.ReadStringNullTerminated((nint)fc->Name);
            var numArray = UIModule.Instance()->GetRaptureAtkModule()->AtkModule.GetNumberArrayData(57);
            if(numArray != null)
            {
                var gil = numArray->IntArray[304];
                if(gil != 0 || S.FCPointsUpdater?.IsFCChestReady() == true)
                {
                    C.FCData[fc->ID].Gil = gil;
                    C.FCData[fc->ID].LastGilUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
								}
						}
						if (Utils.FCPoints != 0)
						{
								C.FCData[fc->ID].FCPoints = Utils.FCPoints;
								C.FCData[fc->ID].FCPointsLastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
						}
				}
        data.WriteOfflineInventoryData();
        C.OfflineData.RemoveAll(x => x.World == "" && x.Name == "Unknown");
        if (saveConfig) EzConfig.Save();
    }

    internal static void WriteOfflineInventoryData(this OfflineCharacterData data)
    {
        data.Ventures = Utils.GetVenturesAmount();
        data.InventorySpace = (uint)Utils.GetInventoryFreeSlotCount();
        data.Ceruleum = InventoryManager.Instance()->GetInventoryItemCount(10155);
        data.RepairKits = InventoryManager.Instance()->GetInventoryItemCount(10373);
    }

    internal static OfflineRetainerData GetData(SeString name, ulong? CID = null) => GetData(name.ToString(), CID);

    internal static OfflineRetainerData GetData(string name, ulong? CID = null)
    {
        var cid = CID ?? Svc.ClientState.LocalContentId;
        if (C.OfflineData.TryGetFirst(x => x.CID == cid, out var data) && data.RetainerData.TryGetFirst(x => x.Name == name, out var rdata))
        {
            return rdata;
        }
        return null;
    }
}
