using AutoRetainer.Offline;
using Dalamud;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Events;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using System.Text.RegularExpressions;

namespace AutoRetainer;

internal static unsafe class Utils
{
    internal static bool TryMatch(this string s, string pattern, out Match match)
    {
        var m = Regex.Match(s, pattern);
        if (m.Success)
        {
            match = m;
            return true;
        }
        else
        {
            match = null;
            return false;
        }
    }

    internal static bool IsCurrentRetainerEnabled()
    {
        return TryGetCurrentRetainer(out var ret) && P.config.SelectedRetainers.TryGetValue(Svc.ClientState.LocalContentId, out var rets) && rets.Contains(ret);
    }

    internal static bool TryGetCurrentRetainer(out string name)
    {
        if (Svc.Condition[ConditionFlag.OccupiedSummoningBell] && ProperOnLogin.PlayerPresent && Svc.Objects.Where(x => x.ObjectKind == ObjectKind.Retainer).OrderBy(x => Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position)).TryGetFirst(out var obj))
        {
            name = obj.Name.ToString();
            return true;
        }
        name = default;
        return false;
    }

    internal static uint GetVenturesAmount()
    {
        return (uint)InventoryManager.Instance()->GetInventoryItemCount(21072);
    }

    internal static bool IsInventoryFree()
    {
        return GetInventoryFreeSlotCount() >= 2;
    }

    internal static string ToTimeString(long seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        var d = ":";
        return $"{t.Hours:D2}{d}{t.Minutes:D2}{d}{t.Seconds:D2}";
    }

    internal static string GetAddonText(uint num)
    {
        return Svc.Data.GetExcelSheet<Addon>().GetRow(num).Text.ToString();
    }

    internal static bool IsRetainerBell(this GameObject o)
    {
        return o != null &&
            (o.ObjectKind == ObjectKind.EventObj || o.ObjectKind == ObjectKind.Housing)
            && o.Name.ToString().EqualsIgnoreCaseAny(Consts.BellName, "リテイナーベル");
    }

    internal static long GetVentureSecondsRemaining(this SeRetainer ret, bool allowNegative = true)
    {
        var x = ret.VentureCompleteTimeStamp - DateTimeOffset.Now.ToUnixTimeSeconds();
        return allowNegative ? x : Math.Max(0, x);
    }

    internal static long GetVentureSecondsRemaining(this OfflineRetainerData ret, bool allowNegative = true)
    {
        var x = ret.VentureEndsAt - DateTimeOffset.Now.ToUnixTimeSeconds();
        return allowNegative? x : Math.Max(0, x);
    }

    internal static bool TryGetRetainerByName(string name, out SeRetainer retainer)
    {
        if (!P.retainerManager.Ready)
        {
            retainer = default;
            return false;
        }
        for(var i = 0; i < P.retainerManager.Count; i++)
        {
            var r = P.retainerManager.Retainer(i);
            if (r.Name.ToString() == name)
            {
                retainer = r;
                return true;
            }
        }
        retainer = default;
        return false;
    }

    internal static int GetInventoryFreeSlotCount()
    {
        InventoryType[] types = new InventoryType[] { InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4 };
        var c = InventoryManager.Instance();
        var slots = 0;
        foreach (var x in types)
        {
            var inv = c->GetInventoryContainer(x);
            for (var i = 0; i < inv->Size; i++)
            {
                if (inv->Items[i].ItemID == 0)
                {
                    slots++;
                }
            }
        }
        return slots;
    }



    internal static bool TryParseRetainerName(string s, out string retainer)
    {
        retainer = default;
        if (!P.retainerManager.Ready)
        {
            return false;
        }
        for (var i = 0; i < P.retainerManager.Count; i++)
        {
            var r = P.retainerManager.Retainer(i);
            var rname = r.Name.ToString();
            if (s.Contains(rname) && (retainer == null || rname.Length > retainer.Length))
            {
                retainer = rname;
            }
        }
        return retainer != default;
    }

    static bool PopupContains(string source, string name)
    {
        if(Svc.Data.Language == ClientLanguage.Japanese)
        {
            return source.Contains($"（{name}）");
        }
        else if (Svc.Data.Language == ClientLanguage.French)
        {
            return source.Contains($"Menu de {name}");
        }
        else if (Svc.Data.Language == ClientLanguage.German)
        {
            return source.Contains($"Du hast {name}");
        }
        else
        {
            return source.Contains($"Retainer: {name}");
        }
    }
}
