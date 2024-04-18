﻿using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainerAPI.Configuration;
using ClickLib.Clicks;
using Dalamud;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Interop;
using ECommons.MathHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CharaData = (string Name, ushort World);

namespace AutoRetainer.Helpers;

internal static unsafe class Utils
{
    internal static bool IsCN => Svc.ClientState.ClientLanguage == (ClientLanguage)4;
    internal static int FCPoints => *(int*)((nint)AgentModule.Instance()->GetAgentByInternalId(AgentId.FreeCompanyCreditShop) + 256);
    internal static float AnimationLock => *(float*)((nint)ActionManager.Instance() + 8);

    internal static bool IsSureNotInFcTerritory()
    {
        var h = HousingManager.Instance();
        if (h->OutdoorTerritory != null)
        {
            var success = false;
            for (int i = 0; i < 30; i++)
            {
                if (P.Memory.OutdoorTerritory_IsEstateResident((nint)h->OutdoorTerritory, (byte)i) == 1) success = true;
            }
            if (!success) return false;
        }
        if (GetFCHouseTerritory() == GetPrivateHouseTerritory()) return false;
        return Player.Territory != GetFCHouseTerritory();
    }

    internal static bool IsSureNotInPrivateTerritory()
    {
        var h = HousingManager.Instance();
        if (h->OutdoorTerritory != null)
        {
            var success = false;
            for (int i = 0; i < 30; i++)
            {
                if (P.Memory.OutdoorTerritory_IsEstateResident((nint)h->OutdoorTerritory, (byte)i) == 1) success = true;
            }
            if (!success) return false;
        }
        if (GetFCHouseTerritory() == GetPrivateHouseTerritory()) return false;
        return Player.Territory != GetPrivateHouseTerritory();
    }

    internal static uint GetFCHouseTerritory()
    {
        foreach (var x in Svc.AetheryteList)
        {
            if (HouseEnterTask.FCAetherytes.Contains(x.AetheryteId) && !x.IsAppartment && !x.IsSharedHouse) return x.TerritoryId;
        }
        return 0;
    }

    internal static uint GetPrivateHouseTerritory()
    {
        foreach (var x in Svc.AetheryteList)
        {
            if (HouseEnterTask.PrivateAetherytes.Contains(x.AetheryteId) && !x.IsAppartment && !x.IsSharedHouse) return x.TerritoryId;
        }
        return 0;
    }

    internal static int LoadedItems => AtkStage.GetSingleton()->GetNumberArrayData()[36]->IntArray[401];

    internal static void ExtraLog(string s)
    {
        if (C.ExtraDebug) PluginLog.Debug(s);
    }

    internal static bool ContainsAllItems<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        return !b.Except(a).Any();
    }

    internal static string Read(byte* ptr)
    {
        return MemoryHelper.ReadStringNullTerminated((nint)ptr);
    }

    internal static float Random { get; private set; } = 1f;
    internal static void RegenerateRandom()
    {
        Random = (float)new Random().NextDouble();
        DebugLog($"Random regenerated: {Random}");
    }

    internal static bool MultiModeOrArtisan => MultiMode.Active || (SchedulerMain.PluginEnabled && SchedulerMain.Reason == PluginEnableReason.Artisan);
    internal static bool IsBusy => P.TaskManager.IsBusy || AutoGCHandin.Operation || AutoLogin.Instance.IsRunning;
    internal static AtkValue ZeroAtkValue = new() { Type = 0, Int = 0 };

    internal static void FixKeys()
    {
        Fix(ref C.EntrustKey);
        Fix(ref C.RetrieveKey);
        Fix(ref C.SellKey);
        Fix(ref C.SellMarketKey);
        Fix(ref C.TempCollectB);
        Fix(ref C.Suppress);
        static void Fix(ref LimitedKeys key)
        {
            if (((Keys)key).EqualsAny(Keys.Control, Keys.ControlKey)) key = LimitedKeys.LeftControlKey;
            if (((Keys)key).EqualsAny(Keys.Shift, Keys.ShiftKey)) key = LimitedKeys.LeftShiftKey;
            if (((Keys)key).EqualsAny(Keys.Alt, Keys.Menu)) key = LimitedKeys.LeftAltKey;
        }
    }

    internal static IEnumerable<string> GetEObjNames(params uint[] values)
    {
        foreach (var x in values)
        {
            yield return Svc.Data.GetExcelSheet<EObjName>().GetRow(x).Singular.ToDalamudString().ExtractText();
        }
    }

    internal static float GetGCSealMultiplier()
    {
        var ret = 1f;
        if (Player.Available)
        {
            if (Player.Object.StatusList.TryGetFirst(x => x.StatusId == 414, out var s)) ret = 1f + (float)s.StackCount / 100f;
            if (Player.Object.StatusList.Any(x => x.StatusId == 1078)) ret = 1.15f;
        }
        return ret > 1f ? ret : 1f;
    }

    internal static bool TryGetCharacterIndex(string name, uint world, out int index)
    {
        index = GetCharacterNames().IndexOf((name, (ushort)world));
        return index >= 0;
    }

    internal static List<CharaData> GetCharacterNames()
    {
        List<CharaData> ret = [];
        /*var data = CSFramework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.GetStringArrayData(1);
        if (data != null)
        {
            for (int i = 60; i < data->AtkArrayData.Size; i++)
            {
                if (data->StringArray[i] == null) break;
                var item = data->StringArray[i];
                if (item != null)
                {
                    var str = MemoryHelper.ReadSeStringNullTerminated((nint)item).ExtractText();
                    if (str == "") break;
                    ret.Add(str);
                }
            }
        }*/
        var agent = AgentLobby.Instance();
        if (agent->AgentInterface.IsAgentActive())
        {
            var charaSpan = agent->LobbyData.CharaSelectEntries.Span;
            for (int i = 0; i < charaSpan.Length; i++)
            {
                var s = charaSpan[i];
                ret.Add(($"{Utils.Read(s.Value->Name)}", s.Value->HomeWorldId));
            }
        }
        return ret;
    }

    internal static string FancyDigits(this int n)
    {
        return n.ToString().ReplaceByChar(Lang.Digits.Normal, Lang.Digits.GameFont);
    }

    internal static int GetJobLevel(this OfflineCharacterData data, uint job)
    {
        var d = Svc.Data.GetExcelSheet<ClassJob>().GetRow(job);
        if (d != null)
        {
            try
            {
                return data.ClassJobLevelArray[d.ExpArrayIndex];
            }
            catch (Exception) { }
        }
        return 0;
    }

    internal static OfflineCharacterData GetCurrentCharacterData()
    {
        return C.OfflineData.FirstOrDefault(x => x.CID == Player.CID);
    }

    internal static bool CanAutoLogin()
    {
        return !Svc.ClientState.IsLoggedIn
            && !Svc.Condition.Any()
            && !P.TaskManager.IsBusy
            && !AutoLogin.Instance.IsRunning
            && IsTitleScreenReady();
    }

    internal static bool IsTitleScreenReady()
    {
        return TryGetAddonByName<AtkUnitBase>("_TitleMenu", out var title)
            && IsAddonReady(title)
            && title->UldManager.NodeListCount > 3
            && title->UldManager.NodeList[3]->Color.A == 0xFF
            && !TryGetAddonByName<AtkUnitBase>("TitleDCWorldMap", out _)
            && !TryGetAddonByName<AtkUnitBase>("TitleConnect", out _);
    }

    internal static OfflineCharacterData GetOfflineCharacterDataFromAdditionalRetainerDataKey(string key)
    {
        var cid = ulong.Parse(key.Split(" ")[0].Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
        return C.OfflineData.FirstOrDefault(x => x.CID == cid);
    }

    internal static OfflineRetainerData GetOfflineRetainerDataFromAdditionalRetainerDataKey(string key)
    {
        return GetOfflineCharacterDataFromAdditionalRetainerDataKey(key).RetainerData.FirstOrDefault(x => x.Name == key.Split(" ")[1]);
    }

    internal static uint GetNextPlannedVenture(this AdditionalRetainerData data)
    {
        var index = data.GetNextPlannedVentureIndex();
        if (index == -1)
        {
            return 0;
        }
        else
        {
            return data.VenturePlan.ListUnwrapped[index];
        }
    }

    internal static int GetNextPlannedVentureIndex(this AdditionalRetainerData data)
    {
        if (data.VenturePlan.ListUnwrapped.Count == 0)
        {
            return -1;
        }
        else
        {
            if (data.VenturePlanIndex >= data.VenturePlan.ListUnwrapped.Count)
            {
                if (data.VenturePlan.PlanCompleteBehavior == PlanCompleteBehavior.Restart_plan)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return (int)data.VenturePlanIndex;
            }
        }
    }

    internal static bool IsLastPlannedVenture(this AdditionalRetainerData data)
    {
        return data.VenturePlanIndex >= data.VenturePlan.ListUnwrapped.Count;
    }

    internal static bool IsVenturePlannerActive(this AdditionalRetainerData data)
    {
        return data.EnablePlanner && data.VenturePlan.ListUnwrapped.Count > 0;
    }

    internal static DateTime DateFromTimeStamp(uint timeStamp)
    {
        const long timeFromEpoch = 62135596800;
        return timeStamp == 0u
            ? DateTime.MinValue
            : new DateTime((timeStamp + timeFromEpoch) * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
    }

    internal static bool IsAnyRetainersCompletedVenture()
    {
        if (!ProperOnLogin.PlayerPresent) return false;
        if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            var selectedRetainers = data.GetEnabledRetainers().Where(z => z.HasVenture);
            return selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= 10);
        }
        return false;
    }

    internal static bool IsAllCurrentCharacterRetainersHaveMoreThan5Mins()
    {
        if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
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

    internal static GameObject GetNearestRetainerBell(out float Distance)
    {
        var currentDistance = float.MaxValue;
        GameObject currentObject = null;
        foreach (var x in Svc.Objects)
        {
            if (x.IsTargetable && (x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Lang.BellName))
            {
                var distance = Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position);
                if (distance < currentDistance)
                {
                    currentDistance = distance;
                    currentObject = x;
                }
            }
        }
        Distance = currentDistance;
        return currentObject;
    }

    internal static GameObject GetReachableRetainerBell(bool extend)
    {
        if (Player.Object is null) return null;

        foreach (var x in Svc.Objects)
        {
            if ((x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Lang.BellName))
            {
                var distance = extend && VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) ? 20f : GetValidInteractionDistance(x);
                if (Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < distance && x.IsTargetable)
                {
                    return x;
                }
            }
        }
        return null;
    }



    internal static bool AnyRetainersAvailableCurrentChara()
    {
        if (C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            return data.GetEnabledRetainers().Any(z => z.GetVentureSecondsRemaining() <= C.UnsyncCompensation);
        }
        return false;
    }

    internal static AdditionalRetainerData GetAdditionalData(ulong cid, string name)
    {
        var key = GetAdditionalDataKey(cid, name, true);
        return C.AdditionalData[key];
    }

    internal static string GetAdditionalDataKey(ulong cid, string name, bool create = true)
    {
        var key = $"#{cid:X16} {name}";
        if (create && !C.AdditionalData.ContainsKey(key))
        {
            C.AdditionalData[key] = new();
        }
        return key;
    }

    public static string UpperCaseStr(Lumina.Text.SeString s, sbyte article = 0)
    {
        if (article == 1)
            return s.ToDalamudString().ToString();

        var sb = new StringBuilder(s.ToDalamudString().ToString());
        var lastSpace = true;
        for (var i = 0; i < sb.Length; ++i)
        {
            if (sb[i] == ' ')
            {
                lastSpace = true;
            }
            else if (lastSpace)
            {
                lastSpace = false;
                sb[i] = char.ToUpperInvariant(sb[i]);
            }
        }

        return sb.ToString();
    }

    internal static bool GenericThrottle => C.UseFrameDelay ? FrameThrottler.Throttle("AutoRetainerGenericThrottle", C.FrameDelay) : EzThrottler.Throttle("AutoRetainerGenericThrottle", C.Delay);
    internal static void RethrottleGeneric(int num)
    {
        if (C.UseFrameDelay)
        {
            FrameThrottler.Throttle("AutoRetainerGenericThrottle", num, true);
        }
        else
        {
            EzThrottler.Throttle("AutoRetainerGenericThrottle", num, true);
        }
    }
    internal static void RethrottleGeneric()
    {
        if (C.UseFrameDelay)
        {
            FrameThrottler.Throttle("AutoRetainerGenericThrottle", C.FrameDelay, true);
        }
        else
        {
            EzThrottler.Throttle("AutoRetainerGenericThrottle", C.Delay, true);
        }
    }

    internal static bool TrySelectSpecificEntry(string text, Func<bool> Throttler = null)
    {
        return TrySelectSpecificEntry(new string[] { text }, Throttler);
    }

    internal static bool TrySelectSpecificEntry(IEnumerable<string> text, Func<bool> Throttler = null)
    {
        return TrySelectSpecificEntry((x) => x.EqualsAny(text), Throttler);
        /*if (TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            var entry = GetEntries(addon).FirstOrDefault(x => x.EqualsAny(text));
            if (entry != null)
            {
                var index = GetEntries(addon).IndexOf(entry);
                if (index >= 0 && IsSelectItemEnabled(addon, index) && (Throttler?.Invoke() ?? GenericThrottle))
                {
                    ClickSelectString.Using((nint)addon).SelectItem((ushort)index);
                    DebugLog($"TrySelectSpecificEntry: selecting {entry}/{index} as requested by {text.Print()}");
                    return true;
                }
            }
        }
        else
        {
            RethrottleGeneric();
        }
        return false;*/
    }

    internal static bool TrySelectSpecificEntry(Func<string, bool> inputTextTest, Func<bool> Throttler = null)
    {
        if (TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            var entry = GetEntries(addon).FirstOrDefault(inputTextTest);
            if (entry != null)
            {
                var index = GetEntries(addon).IndexOf(entry);
                if (index >= 0 && IsSelectItemEnabled(addon, index) && (Throttler?.Invoke() ?? GenericThrottle))
                {
                    ClickSelectString.Using((nint)addon).SelectItem((ushort)index);
                    DebugLog($"TrySelectSpecificEntry: selecting {entry}/{index}");
                    return true;
                }
            }
        }
        else
        {
            RethrottleGeneric();
        }
        return false;
    }

    internal static bool IsSelectItemEnabled(AddonSelectString* addon, int index)
    {
        var step1 = (AtkTextNode*)addon->AtkUnitBase
                    .UldManager.NodeList[2]
                    ->GetComponent()->UldManager.NodeList[index + 1]
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
            }, true);
        }
    }

    internal static float GetValidInteractionDistance(GameObject bell)
    {
        if (bell.ObjectKind == ObjectKind.Housing)
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

    internal static bool IsApartmentEntrance(this GameObject obj)
    {
        return obj.Name.ToString().EqualsIgnoreCase(Lang.ApartmentEntrance);
    }

    internal static GameObject GetNearestEntrance(out float Distance, bool bypassPredefined = false)
    {
        var currentDistance = float.MaxValue;
        GameObject currentObject = null;

        var fcOverride = Data.FreeCompanyHouseEntrance == null ? null : GetEntranceAtLocation(Data.FreeCompanyHouseEntrance.Entrance);
        var pOverride = Data.PrivateHouseEntrance == null ? null : GetEntranceAtLocation(Data.PrivateHouseEntrance.Entrance);

        if (fcOverride != null && pOverride != null)
        {
            var fcd = Vector3.Distance(Player.Object.Position, fcOverride.Position);
            var pd = Vector3.Distance(Player.Object.Position, pOverride.Position);
            if (fcd > pd)
            {
                Distance = pd;
                return pOverride;
            }
            else
            {
                Distance = fcd;
                return fcOverride;
            }
        }
        if (fcOverride != null)
        {
            Distance = Vector3.Distance(Player.Object.Position, fcOverride.Position);
            return fcOverride;
        }
        if (pOverride != null)
        {
            Distance = Vector3.Distance(Player.Object.Position, pOverride.Position);
            return pOverride;
        }

        foreach (var x in Svc.Objects)
        {
            if (x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny([.. Lang.Entrance, Lang.ApartmentEntrance]))
            {
                var distance = Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position);
                if (distance < currentDistance)
                {
                    currentDistance = distance;
                    currentObject = x;
                }
            }
        }
        Distance = currentDistance;
        return currentObject;
    }

    internal static GameObject GetEntranceAtLocation(Vector3 pos)
    {
        foreach (var x in Svc.Objects)
        {
            if (x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny(Lang.Entrance))
            {
                var distance = Vector3.Distance(pos, x.Position);
                if (distance < 1f)
                {
                    return x;
                }
            }
        }
        return null;
    }

    internal static AtkUnitBase* GetSpecificYesno(Predicate<string> compare)
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
                    var text = MemoryHelper.ReadSeString(&textNode->NodeText).ExtractText();
                    if (compare(text))
                    {
                        PluginLog.Verbose($"SelectYesno {text} addon {i} by predicate");
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
                    if (text.EqualsAny(s.Select(x => x.Replace(" ", ""))))
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
        return TryGetCurrentRetainer(out var ret) && C.SelectedRetainers.TryGetValue(Svc.ClientState.LocalContentId, out var rets) && rets.Contains(ret);
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
        return GetInventoryFreeSlotCount() >= C.MultiMinInventorySlots;
    }

    internal static void ResetEscIgnoreByWindows()
    {
        P.SubmarinePointPlanUI.RespectCloseHotkey = !C.IgnoreEsc;
        P.SubmarineUnlockPlanUI.RespectCloseHotkey = !C.IgnoreEsc;
        P.configGui.RespectCloseHotkey = !C.IgnoreEsc;
        P.VenturePlanner.RespectCloseHotkey = !C.IgnoreEsc;
        P.VentureBrowser.RespectCloseHotkey = !C.IgnoreEsc;
        P.LogWindow.RespectCloseHotkey = !C.IgnoreEsc;
        P.DuplicateBlacklistSelector.RespectCloseHotkey = !C.IgnoreEsc;
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
            && o.Name.ToString().EqualsIgnoreCaseAny(Lang.BellName);
    }

    internal static long GetVentureSecondsRemaining(this GameRetainerManager.Retainer ret, bool allowNegative = true)
    {
        var x = ret.VentureCompleteTimeStamp - P.Time;
        return allowNegative ? x : Math.Max(0, x);
    }

    internal static long GetVentureSecondsRemaining(this OfflineRetainerData ret, bool allowNegative = true)
    {
        var x = ret.VentureEndsAt - P.Time;
        return allowNegative ? x : Math.Max(0, x);
    }

    internal static bool TryGetRetainerByName(string name, out GameRetainerManager.Retainer retainer)
    {
        if (!GameRetainerManager.Ready)
        {
            retainer = default;
            return false;
        }
        for (var i = 0; i < GameRetainerManager.Count; i++)
        {
            var r = GameRetainerManager.Retainers[i];
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
        InventoryType[] types = [InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4];
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
        if (!GameRetainerManager.Ready)
        {
            return false;
        }
        for (var i = 0; i < GameRetainerManager.Count; i++)
        {
            var r = GameRetainerManager.Retainers[i];
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
        if (Svc.Data.Language == ClientLanguage.Japanese)
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

    internal static GameObject GetNearestWorkshopEntrance(out float Distance)
    {
        Utils.ExtraLog($"GetNearestWorkshopEntrance: Begin");
        var currentDistance = float.MaxValue;
        GameObject currentObject = null;
        foreach (var x in Svc.Objects)
        {
            Utils.ExtraLog($"GetNearestWorkshopEntrance: Scanning object table: object={x}, targetable={x.IsTargetable}");
            if (x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny(Lang.AdditionalChambersEntrance))
            {
                var distance = Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position);
                Utils.ExtraLog($"GetNearestWorkshopEntrance: check passed, object={x}, targetable={x.IsTargetable}, distance={distance}");
                if (distance < currentDistance)
                {
                    Utils.ExtraLog($"GetNearestWorkshopEntrance: distance is less than current {currentDistance}, assigning from {currentObject}, object={x}, targetable={x.IsTargetable}, distance={distance}");
                    currentDistance = distance;
                    currentObject = x;
                }
            }
        }
        Distance = currentDistance;
        Utils.ExtraLog($"GetNearestWorkshopEntrance: End with distance={currentDistance}, obj={currentObject}");
        return currentObject;
    }
}
