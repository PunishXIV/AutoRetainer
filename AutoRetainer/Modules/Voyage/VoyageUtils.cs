using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage
{
    internal unsafe static class VoyageUtils
    {
        internal static string[] PanelName = new string[] { "Voyage Control Panel" };
        internal static uint[] Workshops = new uint[] { Houses.Company_Workshop_Empyreum, Houses.Company_Workshop_The_Goblet, Houses.Company_Workshop_Mist, Houses.Company_Workshop_Shirogane, Houses.Company_Workshop_The_Lavender_Beds };

        internal static PanelType GetCurrentWorkshopPanelType()
        {
            if (TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GetEntries((AddonSelectString*)addon).Contains("Submersible Management"))
                {
                    return PanelType.TypeSelector;
                }
                var text = MemoryHelper.ReadSeString(&addon->UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText).ExtractText();
                if (text.Contains("Select a submersible."))
                {
                    return PanelType.Submersible;
                }
                if (text.Contains("Select an airship."))
                {
                    return PanelType.Airship;
                }
                return PanelType.Unknown;
            }
            return PanelType.None;
        }

        internal static void Log(string text)
        {
            P.DebugLog($"[Voyage] {text}");
        }

        internal static List<OfflineVesselData> GetVesselData(this OfflineCharacterData data, VoyageType type)
        {
            if (type == VoyageType.Airship) return data.OfflineAirshipData;
            if (type == VoyageType.Submersible) return data.OfflineSubmarineData;
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        internal static HashSet<string> GetEnabledVesselsData(this OfflineCharacterData data, VoyageType type)
        {
            if (type == VoyageType.Airship) return data.EnabledAirships;
            if (type == VoyageType.Submersible) return data.EnabledSubs;
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        internal static HashSet<string> GetFinalizeVesselsData(this OfflineCharacterData data, VoyageType type)
        {
            if (type == VoyageType.Airship) return data.FinalizeAirships;
            if (type == VoyageType.Submersible) return data.FinalizeSubs;
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        internal static bool IsVoyagePanel(this GameObject obj)
        {
            return obj?.Name.ToString().EqualsAny(PanelName) == true;
        }

        internal static bool IsVoyageCondition()
        {
            return Svc.Condition[ConditionFlag.OccupiedInEvent] || Svc.Condition[ConditionFlag.OccupiedInQuestEvent];
        }

        internal static bool IsInVoyagePanel()
        {
            if (IsVoyageCondition() && Svc.Targets.Target.IsVoyagePanel())
            {
                return true;
            }
            return false;
        }

        internal static bool TryGetNearestVoyagePanel(out GameObject obj)
        {
            //Data ID: 2007820
            if (Svc.Objects.TryGetFirst(x => x.Name.ToString().EqualsAny(PanelName) && x.IsTargetable, out var o))
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

        internal static AdditionalVesselData GetAdditionalVesselData(this OfflineCharacterData data,  string name, VoyageType type)
        {
            if (type == VoyageType.Airship)
            {
                if (!data.AdditionalAirshipData.ContainsKey(name)) data.AdditionalAirshipData[name] = new();
                return data.AdditionalAirshipData[name];
            }
            if (type == VoyageType.Submersible)
            {
                if (!data.AdditionalSubmarineData.ContainsKey(name)) data.AdditionalSubmarineData[name] = new();
                return data.AdditionalSubmarineData[name];
            }
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        internal static void WriteOfflineData()
        {
            if (HousingManager.Instance()->WorkshopTerritory != null && C.OfflineData.TryGetFirst(x => x.CID == Player.CID, out var ocd))
            {
                {
                    var vessels = HousingManager.Instance()->WorkshopTerritory->Airship;
                    var temp = new List<OfflineVesselData>();
                    foreach (var x in vessels.DataListSpan)
                    {
                        var name = MemoryHelper.ReadSeStringNullTerminated((nint)x.Name).ExtractText();
                        if (name != "")
                        {
                            temp.Add(new(name, x.ReturnTime));
                            var adata = Data.GetAdditionalVesselData(name, VoyageType.Airship);
                            adata.Level = x.RankId;
                        }
                    }
                    if (temp.Count > 0)
                    {
                        Utils.GetCurrentCharacterData().OfflineAirshipData = temp;
                    }
                }
                {
                    var vessels = HousingManager.Instance()->WorkshopTerritory->Submersible;
                    var temp = new List<OfflineVesselData>();
                    foreach (var x in vessels.DataListSpan)
                    {
                        var name = MemoryHelper.ReadSeStringNullTerminated((nint)x.Name).ExtractText();
                        if (name != "")
                        {
                            temp.Add(new(name, x.ReturnTime));
                            var adata = Data.GetAdditionalVesselData(name, VoyageType.Submersible);
                            adata.Level = x.RankId;
                        }
                    }
                    if (temp.Count > 0)
                    {
                        Utils.GetCurrentCharacterData().OfflineSubmarineData = temp;
                    }
                }
            }
        }

        internal static VoyageType? DetectAddonType(AtkUnitBase* addon)
        {
            var textptr = addon->UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText;
            var text = MemoryHelper.ReadSeString(&textptr).ExtractText();
            if (text.Contains("Select an airship."))
            {
                return VoyageType.Airship;
            }
            if (text.Contains("Select a submersible."))
            {
                return VoyageType.Submersible;
            }
            return null;
        }

        internal static List<int> GetIsVesselNeedsRepair(string name, VoyageType type, out List<string> log) => GetIsVesselNeedsRepair(GetVesselIndexByName(name, type), type, out log);

        internal static List<int> GetIsVesselNeedsRepair(int num, VoyageType type, out List<string> log)
        {
            log = new();
            var ret = new List<int>();
            
            for (var i = 0; i < 4; i++)
            {
                var slot = GetVesselComponent(num, type, i);
                log.Add($"index: {i}, id: {slot->ItemID}, cond: {slot->Condition}");
                if (slot->ItemID == 0)
                {
                    PluginLog.Warning($"Item id for airship component was 0 ({i})");
                    continue;
                }
                if (slot->Condition == 0)
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
            if (type == VoyageType.Airship)
            {
                begin = 30 + vesselIndex * 5;
                itype = InventoryType.HousingInteriorPlacedItems1;
            }
            else if (type == VoyageType.Submersible)
            {
                begin = vesselIndex * 5;
                itype = InventoryType.HousingInteriorPlacedItems2;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
            var index = begin + slotIndex;
            var slot = InventoryManager.Instance()->GetInventoryContainer(itype)->GetInventorySlot(index);
            return slot;
        }

        internal static int GetVesselIndexByName(string name, VoyageType type)
        {
            var index = 0;
            var h = HousingManager.Instance()->WorkshopTerritory;
            if (h != null)
            {
                if (type == VoyageType.Airship)
                {
                    foreach (var x in h->Airship.DataListSpan)
                    {
                        if (MemoryHelper.ReadStringNullTerminated((nint)x.Name) == name)
                        {
                            return index;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                else if (type == VoyageType.Submersible)
                {
                    foreach (var x in h->Submersible.DataListSpan)
                    {
                        if (MemoryHelper.ReadStringNullTerminated((nint)x.Name) == name)
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
            if (t.Days > 0)
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
            return data.GetVesselData(type).Any(x => data.GetEnabledVesselsData(type).Contains(x.Name) && x.ReturnTime != 0 && x.GetRemainingSeconds() < C.UnsyncCompensation);
        }

        internal static string GetNextCompletedVessel(VoyageType type)
        {
            var data = Utils.GetCurrentCharacterData();
            var v = data.GetVesselData(type).Where(x => x.ReturnTime != 0 && x.GetRemainingSeconds() < C.UnsyncCompensation && data.GetEnabledVesselsData(type).Contains(x.Name));
            if (v.Any())
            {
                return v.First().Name;
            }
            return null;
        }

        internal static bool AreAnyVesselsReturnInNext(this OfflineCharacterData data, int minutes, bool all = false) => data.AreAnyVesselsReturnInNext(VoyageType.Airship, minutes, all) || data.AreAnyVesselsReturnInNext(VoyageType.Submersible, minutes, all);

        internal static bool AreAnyVesselsReturnInNext(this OfflineCharacterData data, VoyageType type, int minutes, bool all = false)
        {
            
            if (all)
            {
                var v = data.GetVesselData(type).Where(x => data.GetEnabledVesselsData(type).Contains(x.Name)).All(x => x.ReturnTime != 0 && x.GetRemainingSeconds() < minutes * 60);
                return v;
            }
            else
            {
                var v = data.GetVesselData(type).Where(x => x.ReturnTime != 0 && x.GetRemainingSeconds() < minutes * 60 && data.GetEnabledVesselsData(type).Contains(x.Name));
                if (v.Any())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
