using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage
{
    internal unsafe static class VoyageUtils
    {
        internal static string[] PanelName = new string[] { "Voyage Control Panel" };


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

        internal static bool GetAirshipNeedsRepair(int index)
        {
            for (var i = 30 + index * 5; i < 4; i++)
            {
                var slot = InventoryManager.Instance()->GetInventoryContainer(InventoryType.HousingInteriorPlacedItems1)->GetInventorySlot(i);
                if (slot->ItemID == 0)
                {
                    PluginLog.Warning($"Item id for airship component was 0 ({i})");
                    return false;
                }
                if (slot->Condition == 0) return true;
            }
            return false;
        }

        internal static bool GetSubmarineNeedsRepair(int index)
        {
            for (var i = index * 5; i < 4; i++)
            {
                var slot = InventoryManager.Instance()->GetInventoryContainer(InventoryType.HousingInteriorPlacedItems2)->GetInventorySlot(i);
                if (slot->ItemID == 0)
                {
                    PluginLog.Warning($"Item id for submarine component was 0 ({i})");
                    return false;
                }
                if (slot->Condition == 0) return true;
            }
            return false;
        }

        internal static int GetAirshipIndexByName(string name)
        {
            var h = HousingManager.Instance()->WorkshopTerritory;
            if (h != null)
            {
                var index = 0;
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
            throw new Exception($"Could not retrieve airship's index: {name}");
        }

        internal static int GetSubmarineIndexByName(string name)
        {
            var h = HousingManager.Instance()->WorkshopTerritory;
            if (h != null)
            {
                var index = 0;
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
            throw new Exception($"Could not retrieve submarine's index: {name}");
        }

        internal static bool IsVesselNeedsRepair(string name, VoyageType type)
        {
            if (type == VoyageType.Airship) return GetAirshipNeedsRepair(GetAirshipIndexByName(name));
            if (type == VoyageType.Submersible) return GetSubmarineNeedsRepair(GetSubmarineIndexByName(name));
            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
