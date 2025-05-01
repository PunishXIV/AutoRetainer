using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.PartSwapper;
public unsafe static class PartSwapperUtils
{
    internal static List<(int, uint)> GetIsVesselNeedsPartsSwap(string name, VoyageType type, out List<string> log)
    {
        return GetIsVesselNeedsPartsSwap(VoyageUtils.GetVesselIndexByName(name, type), type, out log);
    }

    internal static List<(int, uint)> GetIsVesselNeedsPartsSwap(int num, VoyageType type, out List<string> log)
    {
        log = [];
        var workshop = HousingManager.Instance()->WorkshopTerritory;

        PluginLog.Debug($"Change - Num: {num}");
        var vesselLevel = (int)workshop->Submersible.Data[num].RankId;

        CheckAndLogParts(num, type, GetPlanInLevelRange(vesselLevel), log, out var requiredChanges);

        return AreRequiredPartsAvailable(requiredChanges) ? requiredChanges : [];
    }

    internal static LevelAndPartsData GetPlanInLevelRange(int vesselLevel)
    {
        foreach(var partsData in C.LevelAndPartsData)
        {
            if(IsLevelInRange(vesselLevel, partsData.MinLevel, partsData.MaxLevel))
            {
                PluginLog.Debug($"Change - {partsData.GUID}");

                return partsData;
            }
        }

        return new LevelAndPartsData();
    }

    internal static bool IsLevelInRange(int level, int minLevel, int maxLevel)
    {
        return minLevel <= level && level <= maxLevel;
    }

    internal static void CheckAndLogParts(int num, VoyageType type, LevelAndPartsData partsData, List<string> log, out List<(int, uint)> changes)
    {
        changes = [];
        for(var slotIndex = 0; slotIndex < 4; slotIndex++)
        {
            var slot = VoyageUtils.GetVesselComponent(num, type, slotIndex);
            var requiredPart = GetRequiredPart(partsData, slotIndex);

            if(slot->ItemId != requiredPart)
            {
                log.Add($"index: {slotIndex}, id: {slot->ItemId}, swap: {requiredPart}");
                changes.Add((slotIndex, requiredPart));
            }
        }
    }

    internal static uint GetRequiredPart(LevelAndPartsData partsData, int slotIndex)
    {
        return slotIndex switch
        {
            0 => (uint)partsData.Part1,
            1 => (uint)partsData.Part2,
            2 => (uint)partsData.Part3,
            3 => (uint)partsData.Part4,
            _ => throw new ArgumentOutOfRangeException(nameof(slotIndex), "Invalid slot index")
        };
    }

    internal static uint GetSubPart(string name, int slotIndex)
    {
        return slotIndex switch
        {
            0 => (uint)Data.AdditionalSubmarineData[name].Part1,
            1 => (uint)Data.AdditionalSubmarineData[name].Part2,
            2 => (uint)Data.AdditionalSubmarineData[name].Part3,
            3 => (uint)Data.AdditionalSubmarineData[name].Part4,
            _ => throw new ArgumentOutOfRangeException(nameof(slotIndex), "Invalid slot index")
        };
    }

    internal static bool AreRequiredPartsAvailable(List<(int, uint)> requiredChanges)
    {
        return requiredChanges.All(change => InventoryManager.Instance()->GetInventoryItemCount(change.Item2) > 0);
    }
}
