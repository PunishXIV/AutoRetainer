using AutoRetainer.Configuration;
using AutoRetainer.Helpers;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.Events;
using ECommons.ExcelServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.Modules;

internal static class OfflineDataManager
{
    internal static void Tick()
    {
        if (Svc.Condition[ConditionFlag.OccupiedSummoningBell])
        {
            if (P.retainerManager.Ready)
            {
                WriteOfflineData();
            }
        }
    }

    internal static void WriteOfflineData()
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
                    Job = ret.ClassJob
                });
            }
        }
        WriteVentureAndInventory();
    }

    internal static void WriteVentureAndInventory()
    {
        if (!ProperOnLogin.PlayerPresent) return;
        OfflineCharacterData data = null;
        if (!P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out data))
        {
            data = new()
            {
                CID = Svc.ClientState.LocalContentId,
            };
            P.config.OfflineData.Add(data);
        }
        data.Ventures = Utils.GetVenturesAmount();
        data.InventorySpace = (uint)Utils.GetInventoryFreeSlotCount();
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
