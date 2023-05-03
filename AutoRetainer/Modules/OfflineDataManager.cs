using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Modules;

internal unsafe static class OfflineDataManager
{
    internal static void Tick()
    {
        if (Svc.Condition[ConditionFlag.OccupiedSummoningBell])
        {
            if (P.retainerManager.Ready)
            {
                WriteOfflineData(false, false);
            }
            if(EzThrottler.Throttle("CalculateItemLevel") && Utils.TryGetCurrentRetainer(out var ret))
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
    }

    internal static void WriteOfflineData(bool writeGatherables, bool saveConfig)
    {
        if (!ProperOnLogin.PlayerPresent) return;
        if (P.config.Blacklist.Any(x => x.CID == Svc.ClientState.LocalContentId)) return;
        if (!P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            data = new()
            {
                CID = Svc.ClientState.LocalContentId,
            };
            P.config.OfflineData.Add(data);
        }
        data.World = ExcelWorldHelper.GetWorldNameById(Svc.ClientState.LocalPlayer.HomeWorld.Id);
        data.Name = Svc.ClientState.LocalPlayer.Name.ToString();
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
        if (P.retainerManager.Ready && P.retainerManager.Count > 0)
        {
            data.RetainerData.Clear();
            for (var i = 0; i < P.retainerManager.Count; i++)
            {
                var ret = P.retainerManager.Retainer(i);
                data.RetainerData.Add(new()
                {
                    Name = ret.Name.ToString(),
                    VentureEndsAt = ret.VentureCompleteTimeStamp,
                    HasVenture = ret.VentureID != 0,
                    Level = ret.Level,
                    Job = ret.ClassJob,
                    VentureID = ret.VentureID, 
                    Gil = ret.Gil,
                });

                for (int p = 0; p < P.retainerManager.Count; p++)
                {
                    if (FFXIVClientStructs.FFXIV.Client.Game.RetainerManager.Instance()->DisplayOrder[p] == i)
                        data.RetainerData[i].DisplayOrder = p;
                }
            }
        }
        data.Ventures = Utils.GetVenturesAmount();
        data.InventorySpace = (uint)Utils.GetInventoryFreeSlotCount();
        P.config.OfflineData.RemoveAll(x => x.World == "" && x.Name == "Unknown");
        if (saveConfig) EzConfig.Save();
    }

    internal static OfflineRetainerData GetData(SeString name, ulong? CID = null) => GetData(name.ToString(), CID);

    internal static OfflineRetainerData GetData(string name, ulong? CID = null)
    {
        var cid = CID ?? Svc.ClientState.LocalContentId;
        if (P.config.OfflineData.TryGetFirst(x => x.CID == cid, out var data) && data.RetainerData.TryGetFirst(x => x.Name == name, out var rdata))
        {
            return rdata;
        }
        return null;
    }
}
