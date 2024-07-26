using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Readers;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Interop;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Modules.Voyage;

internal static unsafe class VoyageUtils
{
    internal static bool DontReassign => (C.TempCollectB != LimitedKeys.None && IsKeyPressed(C.TempCollectB) && !CSFramework.Instance()->WindowInactive);

    internal static uint[] Workshops = [Houses.Company_Workshop_Empyreum, Houses.Company_Workshop_The_Goblet, Houses.Company_Workshop_Mist, Houses.Company_Workshop_Shirogane, Houses.Company_Workshop_The_Lavender_Beds];

    internal static bool ShouldEnterWorkshop() => ((Data.WorkshopEnabled && Data.AreAnyEnabledVesselsReturnInNext(5 * 60, C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)) || (Utils.GetReachableRetainerBell(false) == null)) && Player.IsInHomeWorld;

    internal static SubmarineUnlockPlan GetDefaultSubmarineUnlockPlan(bool New = true)
    {
        var ret = C.SubmarineUnlockPlans.FirstOrDefault(x => x.GUID == C.DefaultSubmarineUnlockPlan);
        if(ret == null && New) return new();
        return ret;
    }

    internal static bool IsNotEnoughSubmarinesEnabled(this OfflineCharacterData data)
    {
        return data.GetVesselData(VoyageType.Submersible).Count > data.GetVesselData(VoyageType.Submersible).Where(x => data.GetEnabledVesselsData(VoyageType.Submersible).Contains(x.Name)).Count();
    }

    internal static bool IsThereNotAssignedSubmarine(this OfflineCharacterData data)
    {
        return data.GetVesselData(VoyageType.Submersible).Where(x => data.GetEnabledVesselsData(VoyageType.Submersible).Contains(x.Name)).Any(x => x.ReturnTime == 0);
    }

    internal static bool AreAnySuboptimalBuildsFound(this OfflineCharacterData data)
    {
        var v = data.GetVesselData(VoyageType.Submersible).Where(x => data.GetEnabledVesselsData(VoyageType.Submersible).Contains(x.Name));
        foreach(var s in v)
        {
            var adata = data.GetAdditionalVesselData(s.Name, VoyageType.Submersible);
            if(adata.IsUnoptimalBuild(out _))
            {
                return true;
            }
        }
        return false;
    }

    internal static bool IsUnoptimalBuild(this AdditionalVesselData adata, out string justification)
    {
        var conf = adata.GetSubmarineBuild().Trim();
        //PluginLog.Information($"{conf}");
        foreach(var x in C.UnoptimalVesselConfigurations)
        {
            if(adata.Level >= x.MinRank && adata.Level <= x.MaxRank)
            {
                if(x.ConfigurationsInvert)
                {
                    //PluginLog.Information($"{conf} vs {x.Configurations.Print()}={conf.EqualsIgnoreCaseAny(x.Configurations)}");
                    if(!conf.EqualsIgnoreCaseAny(x.Configurations))
                    {
                        justification = $"Build is not {x.Configurations.Print()}";
                        return true;
                    }
                }
                else
                {
                    foreach(var inv in x.Configurations)
                    {
                        if(conf.EqualsIgnoreCase(inv))
                        {
                            justification = $"Build is {conf}";
                            return true;
                        }
                    }
                }
            }
        }
        justification = default;
        return false;
    }

    internal static SubmarineExplorationPretty GetSubmarineExploration(uint id)
    {
        return Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(id);
    }

    internal static string GetSubmarineExplorationName(uint id)
    {
        return GetSubmarineExploration(id)?.ConvertDestination();
    }

    internal static string GetMapName(uint id)
    {
        return Svc.Data.GetExcelSheet<SubmarineMap>().GetRow(id)?.Name.ToString();
    }

    internal static int? GetVesselIndex(string name, VoyageType type)
    {
        var w = HousingManager.Instance()->WorkshopTerritory;
        if(w == null) return null;
        var adata = GetAdditionalVesselData(Data, name, type);
        if(adata.IndexOverride > 0) return adata.IndexOverride - 1;
        if(type == VoyageType.Airship)
        {
            var v = w->Airship.Data;
            for(var i = 0; i < v.Length; i++)
            {
                var sub = v[i];
                if(GenericHelpers.Read(sub.Name) == name)
                {
                    return i;
                }
            }
        }
        if(type == VoyageType.Submersible)
        {
            var v = w->Submersible.Data;
            for(var i = 0; i < v.Length; i++)
            {
                var sub = v[i];
                if(GenericHelpers.Read(sub.Name) == name)
                {
                    return i;
                }
            }
        }
        return null;
    }

    internal static List<(uint point, string justification)> GetPrioritizedPointList(this SubmarineUnlockPlan plan)
    {
        var ret = new List<(uint point, string justification)>();
        if(plan.UnlockSubs)
        {
            foreach(var x in Unlocks.PointToUnlockPoint.Where(z => z.Value.Point < 9000 && z.Value.Sub))
            {
                if(!P.SubmarineUnlockPlanUI.IsMapExplored(x.Key, true) && P.SubmarineUnlockPlanUI.IsMapUnlocked(x.Key, true))
                {
                    ret.Add((x.Key, $"submarine slot from {VoyageUtils.GetSubmarineExplorationName(x.Key)}"));
                }
            }
            foreach(var unlock in Unlocks.PointToUnlockPoint.Where(x => x.Value.Sub))
            {
                var path = Unlocks.FindUnlockPath(unlock.Key);
                path.Reverse();
                foreach(var x in path)
                {
                    if(!ret.Any(z => z.point == x.Item2.Point) && !P.SubmarineUnlockPlanUI.IsMapUnlocked(x.Item1, true))
                    {
                        ret.Add((x.Item2.Point, $"{GetSubmarineExplorationName(x.Item1)} on the path to {GetSubmarineExplorationName(unlock.Key)} not unlocked"));
                    }
                }
            }
        }

        foreach(var x in Unlocks.PointToUnlockPoint.Where(z => z.Value.Point < 9000 && !plan.ExcludedRoutes.Contains(z.Key)))
        {
            if(ret.Count > 0 && Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(ret.First().point).Map.Row != Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(x.Key).Map.Row) break;
            if(!P.SubmarineUnlockPlanUI.IsMapUnlocked(x.Key, true) && P.SubmarineUnlockPlanUI.IsMapUnlocked(x.Value.Point, true) && !ret.Any(z => z.point == x.Value.Point))
            {
                ret.Add((x.Value.Point, $"{VoyageUtils.GetSubmarineExplorationName(x.Key)} not unlocked"));
            }
        }
        return ret;
    }

    internal static SubmarineUnlockPlan GetSubmarineUnlockPlanByGuid(string guid)
    {
        return C.SubmarineUnlockPlans.FirstOrDefault(x => x.GUID == guid);
    }

    internal static SubmarinePointPlan GetSubmarinePointPlanByGuid(string guid)
    {
        return C.SubmarinePointPlans.FirstOrDefault(x => x.GUID == guid);
    }

    internal static SubmarineMap GetMap(this SubmarinePointPlan plan)
    {
        if(plan.Points.Count == 0) return null;
        return GetSubmarineExploration(plan.Points[0]).Map.Value;
    }

    internal static string GetPointPlanName(this SubmarinePointPlan plan)
    {
        if(plan == null) return "No or unknown plan selected";
        if(plan.Name.Length > 0) return plan.Name;
        if(plan.Points.Count == 0) return $"Plan {plan.GUID}";
        return $"{plan.GetMap()?.Name}: {plan.Points.Select(x => Svc.Data.GetExcelSheet<SubmarineExplorationPretty>(ClientLanguage.Japanese).GetRow(x).Location.ToString()).Join("→")}";
    }

    internal static uint GetMapId(this SubmarinePointPlan plan) => GetMap(plan)?.RowId ?? 0;

    internal static PanelType GetCurrentWorkshopPanelType()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GetEntries((AddonSelectString*)addon).Any(x => x.EqualsIgnoreCaseAny(Lang.SubmarineManagement)))
            {
                return PanelType.TypeSelector;
            }
            var text = MemoryHelper.ReadSeString(&addon->UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText).ExtractText();
            if(text.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.PanelSubmersible))
            {
                return PanelType.Submersible;
            }
            if(text.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.PanelAirship))
            {
                return PanelType.Airship;
            }
            return PanelType.Unknown;
        }
        return PanelType.None;
    }

    internal static void Log(string text)
    {
        DebugLog($"[Voyage] {text}");
    }

    internal static List<OfflineVesselData> GetVesselData(this OfflineCharacterData data, VoyageType type)
    {
        if(type == VoyageType.Airship) return data.OfflineAirshipData;
        if(type == VoyageType.Submersible) return data.OfflineSubmarineData;
        throw new ArgumentOutOfRangeException(nameof(type));
    }

    internal static HashSet<string> GetEnabledVesselsData(this OfflineCharacterData data, VoyageType type)
    {
        if(type == VoyageType.Airship) return data.EnabledAirships;
        if(type == VoyageType.Submersible) return data.EnabledSubs;
        throw new ArgumentOutOfRangeException(nameof(type));
    }

    /*internal static HashSet<string> GetFinalizeVesselsData(this OfflineCharacterData data, VoyageType type)
    {
        if (type == VoyageType.Airship) return data.FinalizeAirships;
        if (type == VoyageType.Submersible) return data.FinalizeSubs;
        throw new ArgumentOutOfRangeException(nameof(type));
    }*/

    internal static bool IsVoyagePanel(this IGameObject obj)
    {
        return obj?.Name.ToString().EqualsIgnoreCaseAny(Lang.PanelName) == true;
    }

    internal static bool IsVoyageCondition()
    {
        return Svc.Condition[ConditionFlag.OccupiedInEvent] || Svc.Condition[ConditionFlag.OccupiedInQuestEvent];
    }

    internal static bool IsInVoyagePanel()
    {
        if(IsVoyageCondition() && Svc.Targets.Target.IsVoyagePanel())
        {
            return true;
        }
        return false;
    }

    internal static bool TryGetNearestVoyagePanel(out IGameObject obj)
    {
        //Data ID: 2007820
        if(Svc.Objects.TryGetFirst(x => x.Name.ToString().EqualsIgnoreCaseAny(Lang.PanelName) && x.IsTargetable, out var o))
        {
            obj = o;
            return true;
        }
        obj = default;
        return false;
    }

    public static long GetRemainingSeconds(this OfflineVesselData data)
    {
        return data.ReturnTime - P.Time;
    }

    internal static AdditionalVesselData GetAdditionalVesselData(this OfflineCharacterData data, string name, VoyageType type)
    {
        if(type == VoyageType.Airship)
        {
            if(!data.AdditionalAirshipData.ContainsKey(name)) data.AdditionalAirshipData[name] = new();
            return data.AdditionalAirshipData[name];
        }
        if(type == VoyageType.Submersible)
        {
            if(!data.AdditionalSubmarineData.ContainsKey(name)) data.AdditionalSubmarineData[name] = new();
            return data.AdditionalSubmarineData[name];
        }
        throw new ArgumentOutOfRangeException(nameof(type));
    }

    internal static void WriteOfflineData()
    {
        if(HousingManager.Instance()->WorkshopTerritory != null && C.OfflineData.TryGetFirst(x => x.CID == Player.CID, out var ocd))
        {
            ocd.WriteOfflineInventoryData();
            {
                var vessels = HousingManager.Instance()->WorkshopTerritory->Airship;
                var temp = new List<OfflineVesselData>();
                foreach(var x in vessels.Data)
                {
                    var name = x.Name.Read();
                    if(name != "")
                    {
                        temp.Add(new(name, x.ReturnTime));
                        var adata = Data.GetAdditionalVesselData(name, VoyageType.Airship);
                        adata.Level = x.RankId;
                        adata.CurrentExp = x.CurrentExp;
                        adata.NextLevelExp = x.NextLevelExp;
                    }
                }
                if(temp.Count > 0)
                {
                    Data.OfflineAirshipData = temp;
                }
            }
            {
                var vessels = HousingManager.Instance()->WorkshopTerritory->Submersible;
                var temp = new List<OfflineVesselData>();
                for(var i = 0; i < Math.Min(4, vessels.DataPointers.Length); i++)
                {
                    var vessel = vessels.DataPointers[i].Value;
                    if(vessel == null) continue;
                    var name = vessel->Name.Read();
                    if(name != "")
                    {
                        temp.Add(new(name, vessel->ReturnTime));
                        var adata = Data.GetAdditionalVesselData(name, VoyageType.Submersible);
                        adata.Level = vessel->RankId;
                        adata.NextLevelExp = vessel->NextLevelExp;
                        adata.CurrentExp = vessel->CurrentExp;
                        //PluginLog.Debug("Write offline sub data");
                        adata.Part1 = (int)GetVesselComponent(i, VoyageType.Submersible, 0)->ItemId;
                        adata.Part2 = (int)GetVesselComponent(i, VoyageType.Submersible, 1)->ItemId;
                        adata.Part3 = (int)GetVesselComponent(i, VoyageType.Submersible, 2)->ItemId;
                        adata.Part4 = (int)GetVesselComponent(i, VoyageType.Submersible, 3)->ItemId;
                        adata.Points = vessel->CurrentExplorationPoints.ToArray();
                    }
                }
                if(temp.Count > 0)
                {
                    Data.OfflineSubmarineData = temp;
                }
                Data.NumSubSlots = P.SubmarineUnlockPlanUI.GetNumUnlockedSubs() ?? Data.NumSubSlots;
                /*var curSub = CurrentSubmarine.Get();
                if (curSub != null)
                {
                    var adata = Data.GetAdditionalVesselData(Utils.Read(curSub->Name), VoyageType.Submersible);
                    adata.CurrentExp = curSub->CurrentExp;
                    adata.NextLevelExp = curSub->NextLevelExp;
                }*/
            }
        }
    }

    internal static bool IsRetainerBlockedByVoyage()
    {
        if(C.DisableRetainerVesselReturn == 0) return false;
        foreach(var x in C.OfflineData.Where(x => x.WorkshopEnabled))
        {
            if(x.WorkshopEnabled && x.AreAnyEnabledVesselsReturnInNext(C.DisableRetainerVesselReturn * 60)) return true;
        }
        return false;
    }

    internal static string GetSubmarineBuild(this AdditionalVesselData data)
    {
        if(data.Part1 != 0 && data.Part2 != 0 && data.Part3 != 0 && data.Part4 != 0)
        {
            var str = Build.ToIdentifier((ushort)((Items)data.Part1).GetPartId())
                + Build.ToIdentifier((ushort)((Items)data.Part2).GetPartId())
                + Build.ToIdentifier((ushort)((Items)data.Part3).GetPartId())
                + Build.ToIdentifier((ushort)((Items)data.Part4).GetPartId());
            if(str.Length == 8) str = str.Replace("+", "") + "++";
            return " " + str;
        }
        return "";
    }

    internal static VoyageType? DetectAddonType(AtkUnitBase* addon)
    {
        var textptr = addon->UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText;
        var text = MemoryHelper.ReadSeString(&textptr).ExtractText();
        if(text.Contains("Select an airship."))
        {
            return VoyageType.Airship;
        }
        if(text.Contains("Select a submersible."))
        {
            return VoyageType.Submersible;
        }
        return null;
    }

    internal static List<int> GetIsVesselNeedsRepair(string name, VoyageType type, out List<string> log) => GetIsVesselNeedsRepair(GetVesselIndexByName(name, type), type, out log);

    internal static List<int> GetIsVesselNeedsRepair(int num, VoyageType type, out List<string> log)
    {
        log = [];
        var ret = new List<int>();

        for(var i = 0; i < 4; i++)
        {
            var slot = GetVesselComponent(num, type, i);
            log.Add($"index: {i}, id: {slot->ItemId}, cond: {slot->Condition}");
            if(slot->ItemId == 0)
            {
                PluginLog.Warning($"Item id for airship component was 0 ({i})");
                continue;
            }
            if(slot->Condition == 0)
            {
                ret.Add(i);
            }
        }
        return ret;
    }

    internal static InventoryItem* GetVesselComponent(int vesselIndex, VoyageType type, int slotIndex)
    {
        int begin;
        InventoryType itype;
        if(type == VoyageType.Airship)
        {
            begin = 30 + vesselIndex * 5;
            itype = InventoryType.HousingInteriorPlacedItems1;
        }
        else if(type == VoyageType.Submersible)
        {
            begin = vesselIndex * 5;
            itype = InventoryType.HousingInteriorPlacedItems2;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }
        var index = begin + slotIndex;
        var slot = FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryContainer(itype)->GetInventorySlot(index);
        return slot;
    }

    internal static int GetVesselIndexByName(string name, VoyageType type)
    {
        var index = 0;
        var h = HousingManager.Instance()->WorkshopTerritory;
        if(h != null)
        {
            if(type == VoyageType.Airship)
            {
                foreach(var x in h->Airship.Data)
                {
                    if(x.Name.Read() == name)
                    {
                        return index;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            else if(type == VoyageType.Submersible)
            {
                foreach(var x in h->Submersible.Data)
                {
                    if(x.Name.Read() == name)
                    {
                        return index;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
        throw new Exception($"Could not retrieve airship's index: {name}");
    }

    internal static string Seconds2Time(long seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        var dlm = ":";
        if(t.Days > 0)
        {
            return $"{t.Days} days {t.Hours:D2}{dlm}{t.Minutes:D2}{dlm}{t.Seconds:D2}";
        }
        else
        {
            return $"{t.Hours:D2}{dlm}{t.Minutes:D2}{dlm}{t.Seconds:D2}";
        }
    }

    internal static bool AnyEnabledVesselsAvailable(this OfflineCharacterData data)
    {
        return data.AnyEnabledVesselsAvailable(VoyageType.Airship) || data.AnyEnabledVesselsAvailable(VoyageType.Submersible);
    }

    internal static bool AnyEnabledVesselsAvailable(this OfflineCharacterData data, VoyageType type)
    {
        return data.GetVesselData(type).Any(x => data.GetEnabledVesselsData(type).Contains(x.Name) && data.IsVesselAvailable(x, type));
    }

    internal static OfflineVesselData GetOfflineVesselData(this OfflineCharacterData data, string name, VoyageType type)
    {
        if(type == VoyageType.Submersible)
        {
            return data.OfflineSubmarineData.FirstOrDefault(x => x.Name == name);
        }
        else if(type == VoyageType.Airship)
        {
            return data.OfflineAirshipData.FirstOrDefault(x => x.Name == name);
        }
        return null;
    }

    internal static bool IsVesselAvailable(this OfflineCharacterData data, OfflineVesselData x, VoyageType type, int advanceSeconds = 0)
    {
        return (x.ReturnTime != 0 && x.GetRemainingSeconds() < C.UnsyncCompensation + advanceSeconds)
            ||
            (x.ReturnTime == 0 && data.GetAdditionalVesselData(x.Name, type).VesselBehavior.EqualsAny(VesselBehavior.LevelUp, VesselBehavior.Unlock, VesselBehavior.Use_plan, VesselBehavior.Redeploy));
    }

    internal static bool IsVesselNotDeployed(this OfflineVesselData x)
    {
        return x.ReturnTime == 0;
    }

    internal static bool AreAnyEnabledVesselsNotDeployed(this OfflineCharacterData data) => AreAnyEnabledVesselsNotDeployed(data, VoyageType.Airship) && AreAnyEnabledVesselsNotDeployed(data, VoyageType.Submersible);

    internal static bool AreAnyEnabledVesselsNotDeployed(this OfflineCharacterData data, VoyageType type)
    {
        var v = data.GetVesselData(type).Where(x => data.IsVesselAvailable(x, type) && data.GetEnabledVesselsData(type).Contains(x.Name));
        if(v.Any(x => x.IsVesselNotDeployed())) return true;
        return false;
    }

    internal static string GetNextCompletedVessel(VoyageType type)
    {
        var data = Data;
        var v = data.GetVesselData(type).Where(x => data.IsVesselAvailable(x, type) && data.GetEnabledVesselsData(type).Contains(x.Name));
        if(v.Any())
        {
            return v.FirstOrDefault(x => x.ReturnTime != 0)?.Name ?? v.First().Name;
        }
        return null;
    }

    internal static bool AreAnyEnabledVesselsReturnInNext(this OfflineCharacterData data, int seconds, bool all = false, bool ignorePerCharaSetting = false) => data.AreAnyEnabledVesselsReturnInNext(VoyageType.Airship, seconds, all, ignorePerCharaSetting) || data.AreAnyEnabledVesselsReturnInNext(VoyageType.Submersible, seconds, all, ignorePerCharaSetting);

    internal static bool CheckVesselForWaitTreshold(this OfflineCharacterData data, VoyageType type, int seconds)
    {
        if(C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting == 0) return true;
        var completedVesselExists = false;
        var upcomingVesselExists = false;
        foreach(var x in data.GetVesselData(type))
        {
            if(x.GetRemainingSeconds() < seconds)
            {
                completedVesselExists = true;
            }
            else if(x.GetRemainingSeconds() < C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting * 60)
            {
                upcomingVesselExists = true;
            }
        }
        if(completedVesselExists && !upcomingVesselExists) return false;
        return true;
    }

    internal static bool AreAnyEnabledVesselsReturnInNext(this OfflineCharacterData data, VoyageType type, int seconds, bool all = false, bool ignorePerCharaSetting = false)
    {
        if((all || (!ignorePerCharaSetting && data.MultiWaitForAllDeployables)) && data.CheckVesselForWaitTreshold(type, seconds))
        {
            var v = data.GetVesselData(type).Where(x => data.GetEnabledVesselsData(type).Contains(x.Name));
            return v.Any() && v.All(x => data.IsVesselAvailable(x, type, seconds));
        }
        else
        {
            var v = data.GetVesselData(type).Where(x => data.IsVesselAvailable(x, type, seconds) && data.GetEnabledVesselsData(type).Contains(x.Name));
            if(v.Any())
            {
                return true;
            }
        }
        return false;
    }

    internal static bool? CanBeSelected(string FullName)
    {
        if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
        {
            var reader = new ReaderAirShipExploration(addon);
            for(var i = 0; i < reader.Destinations.Count; i++)
            {
                var dest = reader.Destinations[i];
                if(dest.NameFull == FullName)
                {
                    return dest.CanBeSelected;
                }
            }
        }
        return null;
    }

    internal static void SelectRoutePointSafe(string FullOrShortName)
    {
        Log($"Requested selection of {FullOrShortName} point.");
        if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
        {
            var reader = new ReaderAirShipExploration(addon);
            Log($"  Reader initialized with {reader.Destinations.Count} destinations: {reader.Destinations.Select(x => $"{x}").Join("\n")}");
            for(var i = 0; i < reader.Destinations.Count; i++)
            {
                var dest = reader.Destinations[i];
                Log($"  Comparing {i} {dest} with {FullOrShortName}");
                if(FullOrShortName.EqualsIgnoreCaseAny(dest.NameFull, dest.NameShort))
                {
                    Log($"    Found {FullOrShortName}, CanBeSelected = {dest.CanBeSelected}");
                    if(dest.CanBeSelected)
                    {
                        SelectRoutePointSafe(i);
                    }
                    return;
                }
                else
                {
                    Log($"    Negative comparison result");
                }
            }
        }
    }

    internal static void SelectRoutePointSafe(int which)
    {
        Log($"Requested selection of point by ID={which}.");
        if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
        {
            var reader = new ReaderAirShipExploration(addon);
            Log($"  Reader initialized with {reader.Destinations.Count} destinations: {reader.Destinations.Select(x => $"{x}").Join("\n")}");
            if(which >= reader.Destinations.Count) throw new ArgumentOutOfRangeException(nameof(which));
            var dest = reader.Destinations[which];
            Log($"  Destination {dest}");
            if(dest.CanBeSelected)
            {
                VoyageUtils.Log($"  Selecting {dest.NameFull} / {which}");
                P.Memory.SelectRoutePointUnsafe(which);
            }
            else
            {
                VoyageUtils.Log($"  Can't select {dest.NameFull} / {which}, skipping");
            }
        }
    }
}
