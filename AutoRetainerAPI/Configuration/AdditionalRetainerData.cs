using AutoRetainerAPI.Configuration;
using ECommons.DalamudServices;
using System;

namespace AutoRetainerAPI.Configuration;

[Serializable]
public class AdditionalRetainerData
{
    public readonly ulong CreationFrame = Svc.PluginInterface.UiBuilder.FrameCount;
    public bool ShouldSerializeCreationFrame => false;
    public bool EntrustDuplicates = false;
    public bool WithdrawGil = false;
    public int WithdrawGilPercent = 100;
    public bool Deposit = false;
    public VenturePlan VenturePlan = new();
    public string LinkedVenturePlan = "";
    public uint VenturePlanIndex = 0;
    public bool EnablePlanner = false;
    public int Ilvl = -1;
    public int Gathering = -1;
    public int Perception = -1;
}
