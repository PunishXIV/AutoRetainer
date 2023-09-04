using Dalamud.Interface;
using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;

namespace AutoRetainerAPI.Configuration;

[Serializable]
public class OfflineCharacterData
{
    public readonly ulong CreationFrame = Svc.PluginInterface.UiBuilder.FrameCount;
    public bool ShouldSerializeCreationFrame => false;
    public ulong CID = 0;
    public string Name = "Unknown";
    public string World = "";
    public string WorldOverride = null;
    public bool Enabled = false;
    public bool WorkshopEnabled = false;
    public List<OfflineRetainerData> RetainerData = new();
    public bool Preferred = false;
    public uint Ventures = 0;
    public uint InventorySpace = 0;
    public uint VentureCoffers = 0;
    public int ServiceAccount = 0;
    public bool EnableGCArmoryHandin = false; //todo: remove
    public bool ShowRetainersInDisplayOrder = false;
    public bool ShouldSerializeEnableGCArmoryHandin() => false;
    public GCDeliveryType GCDeliveryType = GCDeliveryType.Disabled;
    public HashSet<uint> UnlockedGatheringItems = new();
    public short[] ClassJobLevelArray = new short[30];
    public uint Gil = 0;
    public List<OfflineVesselData> OfflineAirshipData = new();
    public List<OfflineVesselData> OfflineSubmarineData = new();
    public HashSet<string> EnabledAirships = new();
    public HashSet<string> EnabledSubs = new();
    //public HashSet<string> FinalizeAirships = new();
    //public HashSet<string> FinalizeSubs = new();
    public Dictionary<string, AdditionalVesselData> AdditionalAirshipData = new();
    public Dictionary<string, AdditionalVesselData> AdditionalSubmarineData = new();
    public int Ceruleum = 0;
    public int RepairKits = 0;
    public bool ExcludeRetainer = false;
    public bool ExcludeWorkshop = false;
    public bool ExcludeOverlay = false;

    public string Identity => $"{CID}";
    public bool ShouldSerializeIdentity() => false;

    public override string ToString()
    {
        return $"{Name}@{World}";
    }
}
