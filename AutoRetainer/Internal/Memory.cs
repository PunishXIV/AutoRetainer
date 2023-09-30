using AutoRetainer.Scheduler.Handlers;
using ClickLib.Structures;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Net.NetworkInformation;

namespace AutoRetainer.Internal;

internal unsafe class Memory : IDisposable
{
    internal int LastSearchItem = -1;

    delegate ulong InteractWithObjectDelegate(TargetSystem* system, GameObject* obj, bool los);
    Hook<InteractWithObjectDelegate> InteractWithObjectHook;

    delegate byte GetIsGatheringItemGatheredDelegate(ushort item);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 8B D9 8B F9")]
    GetIsGatheringItemGatheredDelegate GetIsGatheringItemGathered;

    internal delegate byte AtkUnitBase_FireCallbackDelegate(AtkUnitBase* a1, int valueCount, AtkValue* values, byte updateState);
    [Signature("E8 ?? ?? ?? ?? 8B 4C 24 20 0F B6 D8", DetourName = nameof(FireCallbackDetour), Fallibility =Fallibility.Fallible)]
    internal Hook<AtkUnitBase_FireCallbackDelegate> FireCallbackHook;


    internal delegate nint OnReceiveMarketPricePacketDelegate(nint a1, nint data);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 40 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 48 8B F8", DetourName = nameof(AddonItemSearchResult_OnRequestedUpdateDelegateDetour), Fallibility =Fallibility.Fallible)]
    internal Hook<OnReceiveMarketPricePacketDelegate> OnReceiveMarketPricePacketHook;

    internal bool IsGatheringItemGathered(uint item) => GetIsGatheringItemGathered((ushort)item) != 0;

    internal Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        if (C.MarketCooldownOverlay) OnReceiveMarketPricePacketHook?.Enable();
        ReceiveRetainerVentureListUpdateHook?.Enable();
    }

    delegate nint ReceiveRetainerVentureListUpdateDelegate(nint a1, int a2, nint a3);
    [Signature("48 89 5C 24 ?? 56 41 56 41 57 48 83 EC 20 8B DA", DetourName =nameof(ReceiveRetainerVentureListUpdateDetour), Fallibility =Fallibility.Infallible)]
    Hook<ReceiveRetainerVentureListUpdateDelegate> ReceiveRetainerVentureListUpdateHook;

    nint ReceiveRetainerVentureListUpdateDetour(nint a1, int a2, nint a3)
    {
        var ret = ReceiveRetainerVentureListUpdateHook.Original(a1, a2, a3);
        PluginLog.Debug($"{a1:X16}, {a2:X8}, {a3:X16}");
        P.ListUpdateFrame = CSFramework.Instance()->FrameCounter;
        return ret;
    }

    nint AddonItemSearchResult_OnRequestedUpdateDelegateDetour(nint a1, nint data)
    {
        var ret = OnReceiveMarketPricePacketHook.Original(a1, data);
        P.MarketCooldownOverlay.UnlockAt = Environment.TickCount64 + 2000;
        return ret;
    }

    internal byte FireCallbackDetour(AtkUnitBase* a1, int valueCount, AtkValue* values, byte updateState)
    {
        if(a1->ID == 118 && valueCount == 2 && values[0].Int == 5)
        {
            PluginLog.Verbose($"Last search item: {values[1].Int}");
            LastSearchItem = values[1].Int;
        }
        return FireCallbackHook.Original(a1, valueCount, values, updateState);
    }

    internal ulong InteractWithObjectDetour(TargetSystem* system, GameObject* obj, bool los)
    {
        DuoLog.Information($"Interacted with {MemoryHelper.ReadSeStringNullTerminated((nint)obj->Name)}, los={los}");
        return InteractWithObjectHook.Original(system, obj, los);
    }

    internal void InstallInteractHook()
    {
        if (InteractWithObjectHook == null)
        {
            InteractWithObjectHook = Svc.Hook.HookFromAddress<InteractWithObjectDelegate>((nint)TargetSystem.Addresses.InteractWithObject.Value, InteractWithObjectDetour);
            InteractWithObjectHook.Enable();
        }
    }

    public void Dispose()
    {
        InteractWithObjectHook?.Dispose();
        FireCallbackHook?.Dispose();
        AddonAirShipExploration_SelectDestinationHook?.Dispose();
        OnReceiveMarketPricePacketHook?.Dispose();
        ReceiveRetainerVentureListUpdateHook?.Dispose();
    }

    internal delegate void AddonAirShipExploration_SelectDestinationDelegate(nint a1, nint a2, AirshipExplorationInputData* a3);
    [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B 78 10", DetourName = nameof(AddonAirShipExploration_SelectDestinationDetour))]
    internal Hook<AddonAirShipExploration_SelectDestinationDelegate> AddonAirShipExploration_SelectDestinationHook;

    internal void AddonAirShipExploration_SelectDestinationDetour(nint a1, nint a2, AirshipExplorationInputData* a3)
    {
        AddonAirShipExploration_SelectDestinationHook.Original(a1, a2, a3);
        var addr = *(nint*)(*(nint*)a3 + 168);
        PluginLog.Debug($"{(nint)a1:X16},{(nint)a2:X16},{(nint)a3:X16}\n{a3->Unk0}, {a3->Unk1}, {(nint)a3->Unk2}, {(nint)(a3->Unk2->Unk0):X16}\n{addr:X16}, {(a3->Unk2->Unk0->Unk0):X16}");
    }

    internal void SelectRoutePointUnsafe(int which)
    {
        if (TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
        {
            var dummyEvent = stackalloc AtkEvent[] { new() };
            var str3 = stackalloc AirshipExplorationInputData3[] { new() { Unk0 = 0x0FFFFFFF } };
            var str2 = stackalloc AirshipExplorationInputData2[] { new() { Unk0 = str3 } };
            var inputData = stackalloc AirshipExplorationInputData[] {
                new()
                {
                    Unk0 = which,
                    Unk1 = 0,
                    Unk2 = str2,
                }
            };
            AddonAirShipExploration_SelectDestinationDetour((nint)addon, (nint)dummyEvent, inputData);
        }
    }
}
