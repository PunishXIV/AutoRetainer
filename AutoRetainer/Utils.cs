using AutoRetainer.Multi;
using AutoRetainer.Offline;
using ClickLib.Clicks;
using Dalamud;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using ECommons.Events;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace AutoRetainer;

internal static unsafe class Utils
{
    internal static bool IsAnyRetainersCompletedVenture()
    {
        if (!ProperOnLogin.PlayerPresent) return false;
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            var selectedRetainers = data.GetEnabledRetainers().Where(z => z.HasVenture);
            return selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= 10);
        }
        return false;
    }

    internal static bool IsAllCurrentCharacterRetainersHaveMoreThan5Mins()
    {
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            foreach (var z in data.GetEnabledRetainers())
            {
                if (z.GetVentureSecondsRemaining() < 5 * 60) return false;
            }
        }
        return true;
    }

    internal static string GetActivePlayerInventoryName()
    {
        {
            if (TryGetAddonByName<AtkUnitBase>("InventoryLarge", out var addon) && addon->IsVisible)
            {
                return "InventoryLarge";
            }
        }
        {
            if (TryGetAddonByName<AtkUnitBase>("InventoryExpansion", out var addon) && addon->IsVisible)
            {
                return "InventoryExpansion";
            }
        }
        return "Inventory";
    }
    internal static (string Name, int EntrustDuplicatesIndex) GetActiveRetainerInventoryName()
    {
        if (TryGetAddonByName<AtkUnitBase>("InventoryRetainerLarge", out var addon) && addon->IsVisible)
        {
            return ("InventoryRetainerLarge", 8);
        }
        return ("InventoryRetainer", 5);
    }

    internal static GameObject GetReachableRetainerBell()
    {
        foreach (var x in Svc.Objects)
        {
            if ((x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Consts.BellName, "リテイナーベル"))
            {
                if (Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < Utils.GetValidInteractionDistance(x) && x.IsTargetable())
                {
                    return x;
                }
            }
        }
        return null;
    }



    internal static bool AnyRetainersAvailableCurrentChara()
    {
        if (P.config.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            return data.GetEnabledRetainers().Any(z => z.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation);
        }
        return false;
    }

    internal static AdditionalRetainerData GetAdditionalData(ulong cid, string name)
    {
        var key = $"#{cid:X16} {name}";
        if (!P.config.AdditionalData.ContainsKey(key))
        {
            P.config.AdditionalData[key] = new();
        }
        return P.config.AdditionalData[key];
    }

    internal static bool GenericThrottle => EzThrottler.Throttle("AutoRetainerGenericThrottle", P.config.Delay);
    internal static void RethrottleGeneric(int num = 200) => EzThrottler.Throttle("AutoRetainerGenericThrottle", num, true);

    internal static bool TrySelectSpecificEntry(string text)
    {
        return TrySelectSpecificEntry(new string[] { text });
    }

    internal static bool TrySelectSpecificEntry(IEnumerable<string> text)
    {
        if (TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            var entry = Utils.GetEntries(addon).FirstOrDefault(x => x.EqualsAny(text));
            if (entry != null)
            {
                var index = Utils.GetEntries(addon).IndexOf(entry);
                if (index >= 0 && Utils.IsSelectItemEnabled(addon, index) && GenericThrottle)
                {
                    ClickSelectString.Using((nint)addon).SelectItem((ushort)index);
                    P.DebugLog($"SelectAssignVenture: selecting {entry}/{index} as requested by {text.Print()}");
                    return true;
                }
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool IsSelectItemEnabled(AddonSelectString* addon, int index)
    {
        var step1 = (AtkTextNode*)addon->AtkUnitBase
                    .UldManager.NodeList[2]
                    ->GetComponent()->UldManager.NodeList[index+1]
                    ->GetComponent()->UldManager.NodeList[3];
        return GenericHelpers.IsSelectItemEnabled(step1);
    }

    internal static List<string> GetEntries(AddonSelectString* addon)
    {
        var list = new List<string>();
        for (int i = 0; i < addon->PopupMenu.PopupMenu.EntryCount; i++)
        {
            list.Add(MemoryHelper.ReadSeStringNullTerminated((nint)addon->PopupMenu.PopupMenu.EntryNames[i]).ExtractText());
        }
        return list;
    }

    internal static void TryNotify(string s)
    {
        if (DalamudReflector.TryGetDalamudPlugin("NotificationMaster", out var instance, true, true))
        {
            Safe(delegate
            {
                instance.GetType().Assembly.GetType("NotificationMaster.TrayIconManager", true).GetMethod("ShowToast").Invoke(null, new object[] { s, P.Name });
            });
        }
    }

    internal static float GetValidInteractionDistance(GameObject bell)
    {
        if(bell.ObjectKind == ObjectKind.Housing)
        {
            return 6.5f;
        }
        else if (Inns.List.Contains(Svc.ClientState.TerritoryType))
        {
            return 4.75f;
        }
        else
        {
            return 4.6f;
        }
    }

    internal static float GetAngleTo(Vector2 pos)
    {
        return (MathHelper.GetRelativeAngle(Svc.ClientState.LocalPlayer.Position.ToVector2(), pos) + Svc.ClientState.LocalPlayer.Rotation.RadToDeg()) % 360;
    }

    internal static GameObject GetNearestEntrance(out float Distance)
    {
        var currentDistance = float.MaxValue;
        GameObject currentObject = null;
        foreach(var x in Svc.Objects)
        {
            if(x.IsTargetable() && x.Name.ToString() == "Entrance")
            {
                var distance = Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position);
                if(distance < currentDistance)
                {
                    currentDistance = distance;
                    currentObject = x;
                }
            }
        }
        Distance = currentDistance;
        return currentObject;
    }

    internal static AtkUnitBase* GetSpecificYesno(params string[] s)
    {
        for (int i = 1; i < 100; i++)
        {
            try
            {
                var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", i);
                if (addon == null) return null;
                if (IsAddonReady(addon))
                {
                    var textNode = addon->UldManager.NodeList[15]->GetAsAtkTextNode();
                    var text = MemoryHelper.ReadSeString(&textNode->NodeText).ExtractText().Replace(" ", "");
                    if(text.EqualsAny(s.Select(x => x.Replace(" ", ""))))
                    {
                        PluginLog.Verbose($"SelectYesno {s.Print()} addon {i}");
                        return addon;
                    }
                }
            }
            catch (Exception e)
            {
                e.Log();
                return null;
            }
        }
        return null;
    }

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
        var x = ret.VentureCompleteTimeStamp - P.Time;
        return allowNegative ? x : Math.Max(0, x);
    }

    internal static long GetVentureSecondsRemaining(this OfflineRetainerData ret, bool allowNegative = true)
    {
        var x = ret.VentureEndsAt - P.Time;
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
