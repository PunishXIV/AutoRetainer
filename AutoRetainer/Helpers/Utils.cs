using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI.Configuration;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using System.Text.RegularExpressions;
using CharaData = (string Name, ushort World);

namespace AutoRetainer.Helpers;

internal static unsafe class Utils
{
    public static int FrameDelay => 10 + C.ExtraFrameDelay;
    internal static bool IsCN => Svc.ClientState.ClientLanguage == (ClientLanguage)4;
    internal static int FCPoints => *(int*)((nint)AgentModule.Instance()->GetAgentByInternalId(AgentId.FreeCompanyCreditShop) + 256);
    internal static float AnimationLock => Player.AnimationLock;

    public static bool IsLockedOut(this OfflineCharacterData characterData)
    {
        var world = ExcelWorldHelper.Get(characterData.WorldOverride ?? characterData.World);
        if(world != null)
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds() < C.LockoutTime.SafeSelect(world.Value.GetRegion(), 0);
        }
        return false;
    }

    public static bool ShouldSkipNPCVendor()
    {
        if(!C.IMSkipVendorIfRetainer) return false;
        if(!C.IMEnableAutoVendor) return false;
        if(C.MultiModeType == MultiModeType.Submersibles) return false;
        if(Data == null) return false;
        if(!Data.Enabled) return false;
        if(Data.GetEnabledRetainers().Length == 0) return false;
        return true;
    }

    private static bool IsNullOrEmpty(this string s) => GenericHelpers.IsNullOrEmpty(s);

    public static void EnsureEnhancedLoginIsOff()
    {
        /*try
        {
            if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "HaselTweaks" && x.IsLoaded))
            {
                if(DalamudReflector.TryGetDalamudPlugin("HaselTweaks", out var instance, out var context, false, true))
                {
                    var configWindow = ReflectionHelper.CallStatic(context.Assemblies, "HaselCommon.Service", [], "Get", ["HaselTweaks.Windows.PluginWindow"], []);
                    var tweaks = (System.Collections.IEnumerable)configWindow.GetFoP("Tweaks");
                    foreach(var x in tweaks)
                    {
                        if(x.GetFoP<string>("InternalName") == "EnhancedLoginLogout" && x.GetFoP<int>("Status") == 5)
                        {
                            configWindow.GetFoP("TweakManager").Call("UserDisableTweak", [x], true);
                            new PopupWindow(() =>
                            {
                                ImGuiEx.Text($"""
                                    Enhanced Login/Logout from HaselTweaks plugin has been detected.
                                    It is not compatible with AutoRetainer and has been disabled.
                                    """);
                            });
                        }
                    }
                }
            }
        }
        catch(Exception e)
        {
            e.Log();
        }*/
    }

    public static bool ShouldWaitForAllWhenLoggedIn(this OfflineCharacterData data)
    {
        return C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn && (C.MultiModeWorkshopConfiguration.MultiWaitForAll || data.MultiWaitForAllDeployables);
    }

    public static void EnqueueVendorItemsByRetainer()
    {
        for(var i = 0; i < GameRetainerManager.Count; i++)
        {
            var ret = GameRetainerManager.Retainers[i];
            if(ret.Available)
            {
                P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                TaskVendorItems.Enqueue();

                if(C.RetainerMenuDelay > 0)
                {
                    TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                }
                P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                P.TaskManager.Enqueue(RetainerHandlers.ConfirmCantBuyback);
                break;
            }
        }
    }

    public static bool GetAllowFcTeleportForRetainers(this OfflineCharacterData data) => data.IsTeleportEnabled() && data.GetIsTeleportEnabledForRetainers() && (data.TeleportOptionsOverride.RetainersFC ?? C.GlobalTeleportOptions.RetainersFC);
    public static bool GetAllowPrivateTeleportForRetainers(this OfflineCharacterData data) => data.IsTeleportEnabled() && data.GetIsTeleportEnabledForRetainers() && (data.TeleportOptionsOverride.RetainersPrivate ?? C.GlobalTeleportOptions.RetainersPrivate);
    public static bool GetAllowApartmentTeleportForRetainers(this OfflineCharacterData data) => data.IsTeleportEnabled() && data.GetIsTeleportEnabledForRetainers() && (data.TeleportOptionsOverride.RetainersApartment ?? C.GlobalTeleportOptions.RetainersApartment);

    public static bool GetAllowFcTeleportForSubs(this OfflineCharacterData data) => data.IsTeleportEnabled() && (data.TeleportOptionsOverride.Deployables ?? C.GlobalTeleportOptions.Deployables);

    public static bool IsTeleportEnabled(this OfflineCharacterData data) => data.TeleportOptionsOverride.Enabled ?? C.GlobalTeleportOptions.Enabled;

    public static bool GetIsTeleportEnabledForRetainers(this OfflineCharacterData data) => data.TeleportOptionsOverride.Retainers ?? C.GlobalTeleportOptions.Retainers;

    public static bool GetAreTeleportSettingsOverriden(this OfflineCharacterData data)
    {
        return data.TeleportOptionsOverride.Deployables != null
            || data.TeleportOptionsOverride.Enabled != null
            || data.TeleportOptionsOverride.Retainers != null
            || data.TeleportOptionsOverride.RetainersApartment != null
            || data.TeleportOptionsOverride.RetainersFC != null
            || data.TeleportOptionsOverride.RetainersPrivate != null;
    }

    public static long GetRemainingSessionMiliSeconds() => P.TimeLaunched[0] + 3 * 24 * 60 * 60 * 1000 - DateTimeOffset.Now.ToUnixTimeMilliseconds();

    public static readonly InventoryType[] RetainerInventories = [InventoryType.RetainerPage1, InventoryType.RetainerPage2, InventoryType.RetainerPage3, InventoryType.RetainerPage4, InventoryType.RetainerPage5, InventoryType.RetainerPage6, InventoryType.RetainerPage7];
    public static readonly InventoryType[] RetainerInventoriesWithCrystals = [InventoryType.RetainerPage1, InventoryType.RetainerPage2, InventoryType.RetainerPage3, InventoryType.RetainerPage4, InventoryType.RetainerPage5, InventoryType.RetainerPage6, InventoryType.RetainerPage7, InventoryType.RetainerCrystals];
    public static readonly InventoryType[] PlayerInvetories = [InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4];
    public static readonly InventoryType[] PlayerInvetoriesWithCrystals = [InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4, InventoryType.Crystals];
    public static readonly InventoryType[] PlayerArmory = [InventoryType.ArmoryOffHand, InventoryType.ArmoryHead, InventoryType.ArmoryBody, InventoryType.ArmoryHands, InventoryType.ArmoryWaist, InventoryType.ArmoryLegs, InventoryType.ArmoryFeets, InventoryType.ArmoryEar, InventoryType.ArmoryNeck, InventoryType.ArmoryWrist, InventoryType.ArmoryRings, InventoryType.ArmorySoulCrystal, InventoryType.ArmoryMainHand];

    public static InventoryType[] GetAllowedInventories(this EntrustPlan plan)
    {
        return plan.AllowEntrustFromArmory ? [.. PlayerInvetoriesWithCrystals, .. PlayerArmory] : PlayerInvetoriesWithCrystals;
    }

    public static List<(uint ID, uint Quantity)> GetCapturedInventoryState(IEnumerable<InventoryType> inventoryTypes)
    {
        var ret = new List<(uint ID, uint Quantity)>();
        foreach(var type in inventoryTypes)
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(type);
            for(var i = 0; i < inv->Size; i++)
            {
                var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                ret.Add((item->ItemId, (uint)item->Quantity));
            }
        }
        return ret;
    }

    /// <summary>
    /// Request all unique items from select inventories
    /// </summary>
    /// <param name="inventoryTypes"></param>
    /// <returns></returns>
    public static HashSet<uint> GetItemsInInventory(IEnumerable<InventoryType> inventoryTypes)
    {
        var ret = new HashSet<uint>();
        foreach(var type in inventoryTypes)
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(type);
            for(var i = 0; i < inv->Size; i++)
            {
                var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                if(item->ItemId != 0)
                {
                    ret.Add(item->ItemId);
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// Gets total item count of certain item across all inventories
    /// </summary>
    /// <param name="inventoryTypes"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public static int GetItemCount(IEnumerable<InventoryType> inventoryTypes, uint itemId)
    {
        var ret = 0;
        foreach(var type in inventoryTypes)
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(type);
            for(var i = 0; i < inv->Size; i++)
            {
                var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                if(item->ItemId == itemId)
                {
                    ret += (int)item->Quantity;
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// Gets amount of items that can fit into inventories
    /// </summary>
    /// <param name="inventoryTypes"></param>
    /// <param name="itemId"></param>
    /// <param name="isHq"></param>
    /// <returns></returns>
    public static uint GetAmountThatCanFit(IEnumerable<InventoryType> inventoryTypes, uint itemId, bool isHq, out List<string> debugData)
    {
        uint ret = 0;
        var data = ExcelItemHelper.Get(itemId);
        debugData = [];
        if(data == null) return 0;
        if(data.Value.ItemUICategory.RowId == 59)//crystal special handling
        {
            foreach(var type in inventoryTypes)
            {
                var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                for(var i = 0; i < inv->Size; i++)
                {
                    var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                    if(item->ItemId == itemId)
                    {
                        ret += (uint)(data.Value.StackSize - item->Quantity);
                        debugData.Add($"[TED] [CrystalDebugData] in {type} slot {i} found incomplete stack: {ExcelItemHelper.GetName(itemId, true)} q={item->Quantity} canFit={ret}");
                        return ret;
                    }
                }
            }
            return data.Value.StackSize;
        }
        else
        {
            foreach(var type in inventoryTypes)
            {
                if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                for(var i = 0; i < inv->Size; i++)
                {
                    var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                    if(item->ItemId == itemId && item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality) == isHq && !item->Flags.HasFlag(InventoryItem.ItemFlags.Collectable))
                    {
                        if(data.Value.IsUnique) return 0;
                        debugData.Add($"[TED] [DebugData] in {type} slot {i} found incomplete stack: {ExcelItemHelper.GetName(itemId, true)} q={item->Quantity} canFit={ret}");
                        ret += (uint)(data.Value.StackSize - item->Quantity);
                    }
                    else if(item->ItemId == 0)
                    {
                        debugData.Add($"[TED] [DebugData] in {type} slot {i} is empty, canFit={data.Value.StackSize}");
                        ret += data.Value.StackSize;
                    }
                }
            }
        }
        return ret;
    }

    public static bool IsItemSellableByHardList(Number item, Number quantity)
    {
        if(C.IMProtectList.Contains(item)) return false;
        if(C.IMAutoVendorHard.Contains(item))
        {
            if(C.IMAutoVendorHardIgnoreStack.Contains(item)) return true;
            return quantity < C.IMAutoVendorHardStackLimit;
        }
        else
        {
            return false;
        }
    }

    public static bool? WaitForScreen() => IsScreenReady();

    internal static void ExtraLog(string s)
    {
        if(C.ExtraDebug) PluginLog.Debug(s);
    }

    internal static bool ContainsAllItems<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        return !b.Except(a).Any();
    }

    internal static float Random { get; private set; } = 1f;
    internal static void RegenerateRandom()
    {
        Random = (float)new Random().NextDouble();
        DebugLog($"Random regenerated: {Random}");
    }

    internal static bool MultiModeOrArtisan => MultiMode.Active || (SchedulerMain.PluginEnabled && SchedulerMain.Reason == PluginEnableReason.Artisan);
    internal static bool IsBusy => P.TaskManager.IsBusy || AutoGCHandin.Operation || S.LifestreamIPC.IsBusy();
    internal static AtkValue ZeroAtkValue = new() { Type = 0, Int = 0 };

    internal static IEnumerable<string> GetEObjNames(params uint[] values)
    {
        foreach(var x in values)
        {
            yield return Svc.Data.GetExcelSheet<EObjName>().GetRow(x).Singular.GetText();
        }
    }

    internal static float GetGCSealMultiplier()
    {
        var ret = 1f;
        if(Player.Available)
        {
            if(Player.Object.StatusList.TryGetFirst(x => x.StatusId == 414, out var s)) ret = 1f + (float)s.StackCount / 100f;
            if(Player.Object.StatusList.Any(x => x.StatusId == 1078)) ret = 1.15f;
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
                    var str = MemoryHelper.ReadSeStringNullTerminated((nint)item).GetText();
                    if (str == "") break;
                    ret.Add(str);
                }
            }
        }*/
        var agent = AgentLobby.Instance();
        if(agent->AgentInterface.IsAgentActive())
        {
            var charaSpan = agent->LobbyData.CharaSelectEntries.AsSpan();
            for(var i = 0; i < charaSpan.Length; i++)
            {
                var s = charaSpan[i];
                ret.Add(($"{s.Value->Name.Read()}", s.Value->HomeWorldId));
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
        var d = Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault(job);
        if(d != null)
        {
            try
            {
                return data.ClassJobLevelArray.SafeSelect(d.Value.ExpArrayIndex);
            }
            catch(Exception) { }
        }
        return 0;
    }

    internal static OfflineCharacterData GetCurrentCharacterData()
    {
        return C.OfflineData.FirstOrDefault(x => x.CID == Player.CID);
    }

    internal static bool CanAutoLogin()
    {
        return CanAutoLoginFromTaskManager() && !P.TaskManager.IsBusy;
    }

    internal static bool CanAutoLoginFromTaskManager()
    {
        return !Svc.ClientState.IsLoggedIn
            && !Svc.Condition.Any()
            && IsTitleScreenReady();
    }

    internal static bool IsTitleScreenReady()
    {
        return TryGetAddonByName<AtkUnitBase>("_TitleMenu", out var title)
            && IsAddonReady(title)
            && title->UldManager.NodeListCount > 3
            && title->UldManager.NodeList[7]->IsVisible()
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
        if(index == -1)
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
        if(data.VenturePlan.ListUnwrapped.Count == 0)
        {
            return -1;
        }
        else
        {
            if(data.VenturePlanIndex >= data.VenturePlan.ListUnwrapped.Count)
            {
                if(data.VenturePlan.PlanCompleteBehavior == PlanCompleteBehavior.Restart_plan)
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
        if(!ProperOnLogin.PlayerPresent) return false;
        if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            var selectedRetainers = data.GetEnabledRetainers().Where(z => z.HasVenture);
            return selectedRetainers.Any(z => z.GetVentureSecondsRemaining() <= 10);
        }
        return false;
    }

    internal static bool IsAllCurrentCharacterRetainersHaveMoreThan5Mins()
    {
        if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
        {
            foreach(var z in data.GetEnabledRetainers())
            {
                if(z.GetVentureSecondsRemaining() < 5 * 60) return false;
            }
        }
        return true;
    }

    internal static string GetActivePlayerInventoryName()
    {
        {
            if(TryGetAddonByName<AtkUnitBase>("InventoryLarge", out var addon) && addon->IsVisible)
            {
                return "InventoryLarge";
            }
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("InventoryExpansion", out var addon) && addon->IsVisible)
            {
                return "InventoryExpansion";
            }
        }
        return "Inventory";
    }
    internal static (string Name, int EntrustDuplicatesIndex) GetActiveRetainerInventoryName()
    {
        if(TryGetAddonByName<AtkUnitBase>("InventoryRetainerLarge", out var addon) && addon->IsVisible)
        {
            return ("InventoryRetainerLarge", 8);
        }
        return ("InventoryRetainer", 5);
    }

    internal static IGameObject GetNearestRetainerBell(out float Distance)
    {
        var currentDistance = float.MaxValue;
        IGameObject currentObject = null;
        foreach(var x in Svc.Objects)
        {
            if(x.IsTargetable && (x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Lang.BellName))
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

    internal static IGameObject GetReachableRetainerBell(bool extend)
    {
        if(Player.Object is null) return null;

        foreach(var x in Svc.Objects)
        {
            if((x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Lang.BellName))
            {
                var distance = extend && VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) ? 20f : GetValidInteractionDistance(x);
                if(Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < distance && x.IsTargetable)
                {
                    return x;
                }
            }
        }
        return null;
    }



    internal static bool AnyRetainersAvailableCurrentChara()
    {
        if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var data))
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
        if(create && !C.AdditionalData.ContainsKey(key))
        {
            C.AdditionalData[key] = new();
        }
        return key;
    }

    public static string UpperCaseStr(ReadOnlySeString s, sbyte article = 0)
    {
        if(article == 1)
            return s.ToDalamudString().ToString();

        var sb = new StringBuilder(s.ToDalamudString().ToString());
        var lastSpace = true;
        for(var i = 0; i < sb.Length; ++i)
        {
            if(sb[i] == ' ')
            {
                lastSpace = true;
            }
            else if(lastSpace)
            {
                lastSpace = false;
                sb[i] = char.ToUpperInvariant(sb[i]);
            }
        }

        return sb.ToString();
    }

    internal static bool GenericThrottle => FrameThrottler.Throttle("AutoRetainerGenericThrottle", Utils.FrameDelay);
    internal static void RethrottleGeneric(int num)
    {
        FrameThrottler.Throttle("AutoRetainerGenericThrottle", num, true);
    }
    internal static void RethrottleGeneric()
    {
        FrameThrottler.Throttle("AutoRetainerGenericThrottle", Utils.FrameDelay, true);
    }

    internal static bool TrySelectSpecificEntry(string text, Func<bool> Throttler = null)
    {
        return TrySelectSpecificEntry(new string[] { text }, Throttler);
    }

    internal static bool TrySelectSpecificEntry(IEnumerable<string> text, Func<bool> Throttler = null)
    {
        return TrySelectSpecificEntry((x) => x.StartsWithAny(text), Throttler);
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
        if(TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
        {
            if(new AddonMaster.SelectString(addon).Entries.TryGetFirst(x => inputTextTest(x.Text), out var entry))
            {
                if((Throttler?.Invoke() ?? GenericThrottle))
                {
                    entry.Select();
                    DebugLog($"TrySelectSpecificEntry: selecting {entry}");
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

    internal static List<string> GetEntries(AddonSelectString* addon)
    {
        var list = new List<string>();
        for(var i = 0; i < addon->PopupMenu.PopupMenu.EntryCount; i++)
        {
            list.Add(MemoryHelper.ReadSeStringNullTerminated((nint)addon->PopupMenu.PopupMenu.EntryNames[i]).GetText());
        }
        return list;
    }

    internal static void TryNotify(string s)
    {
        if(DalamudReflector.TryGetDalamudPlugin("NotificationMaster", out var instance, true, true))
        {
            Safe(delegate
            {
                instance.GetType().Assembly.GetType("NotificationMaster.TrayIconManager", true).GetMethod("ShowToast").Invoke(null, new object[] { s, P.Name });
            }, true);
        }
    }

    internal static float GetValidInteractionDistance(IGameObject bell)
    {
        if(bell.ObjectKind == ObjectKind.Housing)
        {
            return 6.5f;
        }
        else if(Inns.List.Contains(Svc.ClientState.TerritoryType))
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

    internal static bool IsApartmentEntrance(this IGameObject obj)
    {
        return obj.Name.ToString().EqualsIgnoreCase(Lang.ApartmentEntrance);
    }

    internal static IGameObject GetNearestEntrance(out float Distance)
    {
        var currentDistance = float.MaxValue;
        IGameObject currentObject = null;

        foreach(var x in Svc.Objects)
        {
            if(x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny([.. Lang.Entrance/*, Lang.ApartmentEntrance*/]))
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
        if(Distance > 20) return null;
        return currentObject;
    }

    internal static IGameObject GetEntranceAtLocation(Vector3 pos)
    {
        foreach(var x in Svc.Objects)
        {
            if(x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny(Lang.Entrance))
            {
                var distance = Vector3.Distance(pos, x.Position);
                if(distance < 1f)
                {
                    return x;
                }
            }
        }
        return null;
    }

    internal static AtkUnitBase* GetSpecificYesno(Predicate<string> compare)
    {
        for(var i = 1; i < 100; i++)
        {
            try
            {
                var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", i);
                if(addon == null) return null;
                if(IsAddonReady(addon))
                {
                    var textNode = addon->UldManager.NodeList[15]->GetAsAtkTextNode();
                    var text = GenericHelpers.ReadSeString(&textNode->NodeText).GetText();
                    if(compare(text))
                    {
                        PluginLog.Verbose($"SelectYesno {text} addon {i} by predicate");
                        return addon;
                    }
                }
            }
            catch(Exception e)
            {
                e.Log();
                return null;
            }
        }
        return null;
    }

    internal static AtkUnitBase* GetSpecificYesno(params string[] s)
    {
        for(var i = 1; i < 100; i++)
        {
            try
            {
                var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", i);
                if(addon == null) return null;
                if(IsAddonReady(addon))
                {
                    var textNode = addon->UldManager.NodeList[15]->GetAsAtkTextNode();
                    var text = textNode->NodeText.GetText().Cleanup();
                    if(text.ContainsAny(s.Select(x => x.Cleanup())))
                    {
                        PluginLog.Verbose($"SelectYesno {s.Print()} addon {i}");
                        return addon;
                    }
                }
            }
            catch(Exception e)
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
        if(m.Success)
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
        if(Svc.Condition[ConditionFlag.OccupiedSummoningBell] && ProperOnLogin.PlayerPresent && Svc.Objects.Where(x => x.ObjectKind == ObjectKind.Retainer).OrderBy(x => Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position)).TryGetFirst(out var obj))
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
        P.AutoRetainerWindow.RespectCloseHotkey = !C.IgnoreEsc;
        P.VenturePlanner.RespectCloseHotkey = !C.IgnoreEsc;
        P.VentureBrowser.RespectCloseHotkey = !C.IgnoreEsc;
        P.LogWindow.RespectCloseHotkey = !C.IgnoreEsc;
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

    internal static bool IsRetainerBell(this IGameObject o)
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
        if(!GameRetainerManager.Ready)
        {
            retainer = default;
            return false;
        }
        for(var i = 0; i < GameRetainerManager.Count; i++)
        {
            var r = GameRetainerManager.Retainers[i];
            if(r.Name.ToString() == name)
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
        foreach(var x in types)
        {
            var inv = c->GetInventoryContainer(x);
            for(var i = 0; i < inv->Size; i++)
            {
                if(inv->Items[i].ItemId == 0)
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
        if(!GameRetainerManager.Ready)
        {
            return false;
        }
        for(var i = 0; i < GameRetainerManager.Count; i++)
        {
            var r = GameRetainerManager.Retainers[i];
            var rname = r.Name.ToString();
            if(s.Contains(rname) && (retainer == null || rname.Length > retainer.Length))
            {
                retainer = rname;
            }
        }
        return retainer != default;
    }

    private static bool PopupContains(string source, string name)
    {
        if(Svc.Data.Language == ClientLanguage.Japanese)
        {
            return source.Contains($"（{name}）");
        }
        else if(Svc.Data.Language == ClientLanguage.French)
        {
            return source.Contains($"Menu de {name}");
        }
        else if(Svc.Data.Language == ClientLanguage.German)
        {
            return source.Contains($"Du hast {name}");
        }
        else
        {
            return source.Contains($"Retainer: {name}");
        }
    }

    internal static IGameObject GetNearestWorkshopEntrance(out float Distance)
    {
        Utils.ExtraLog($"GetNearestWorkshopEntrance: Begin");
        var currentDistance = float.MaxValue;
        IGameObject currentObject = null;
        foreach(var x in Svc.Objects)
        {
            Utils.ExtraLog($"GetNearestWorkshopEntrance: Scanning object table: object={x}, targetable={x.IsTargetable}");
            if(x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny(Lang.AdditionalChambersEntrance))
            {
                var distance = Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position);
                Utils.ExtraLog($"GetNearestWorkshopEntrance: check passed, object={x}, targetable={x.IsTargetable}, distance={distance}");
                if(distance < currentDistance)
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
