using AutoRetainer.Internal.InventoryManagement;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.ExcelServices;
using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Internal;

internal unsafe class Memory : IDisposable
{
    internal int LastSearchItem = -1;

    private delegate ulong InteractWithObjectDelegate(TargetSystem* system, GameObject* obj, bool los);

    private Hook<InteractWithObjectDelegate> InteractWithObjectHook;

    private delegate byte GetIsGatheringItemGatheredDelegate(ushort item);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 8B D9 8B F9")]
    private GetIsGatheringItemGatheredDelegate GetIsGatheringItemGathered;

    internal delegate nint OnReceiveMarketPricePacketDelegate(nint a1, nint data);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B 0D ?? ?? ?? ?? 48 8B DA E8", DetourName = nameof(AddonItemSearchResult_OnRequestedUpdateDelegateDetour), Fallibility = Fallibility.Fallible)]
    internal Hook<OnReceiveMarketPricePacketDelegate> OnReceiveMarketPricePacketHook;

    internal delegate byte OutdoorTerritory_IsEstateResidentDelegate(nint a1, byte a2);
    [Signature("8B 05 ?? ?? ?? ?? 44 0F B6 D2 44 8B 81")]
    internal OutdoorTerritory_IsEstateResidentDelegate OutdoorTerritory_IsEstateResident;

    internal delegate void RetainerItemCommandDelegate(nint AgentRetainerItemCommandModule, uint slot, InventoryType inventoryType, uint a4, RetainerItemCommand command);
    internal EzHook<RetainerItemCommandDelegate> RetainerItemCommandHook;

    public nint* MyAccountData = (nint*)Svc.SigScanner.GetStaticAddressFromSig("48 8B 3D ?? ?? ?? ?? 48 85 FF 74 69");
    public ulong* MyAccountId => (ulong*)(*MyAccountData + 8);

    public delegate nint AddonGrandCompanySupplyList_SetExchangeModeDelegate(nint addon, int mode);
    public AddonGrandCompanySupplyList_SetExchangeModeDelegate AddonGrandCompanySupplyList_SetExchangeMode = EzDelegate.Get<AddonGrandCompanySupplyList_SetExchangeModeDelegate>("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 8B D6 48 8D 4D F7");

    internal bool IsGatheringItemGathered(uint item)
    {
        return GetIsGatheringItemGathered((ushort)item) != 0;
    }

    internal Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        EzSignatureHelper.Initialize(this);
        if(C.MarketCooldownOverlay) OnReceiveMarketPricePacketHook?.Enable();
        ReceiveRetainerVentureListUpdateHook?.Enable();
        RetainerItemCommandHook = new("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 8B 5C 24 ?? 41 8B F0", RetainerItemCommandDetour, false);
    }

    internal void RetainerItemCommandDetour(nint AgentRetainerItemCommandModule, uint slot, InventoryType inventoryType, uint a4, RetainerItemCommand command)
    {
        try
        {
            DebugLog($"RetainerItemCommandDetour: {AgentRetainerItemCommandModule:X16}, slot={slot}, type={inventoryType}, a4={a4}, command={command}");
        }
        catch(Exception e)
        {
            e.Log();
        }
        RetainerItemCommandHook.Original(AgentRetainerItemCommandModule, slot, inventoryType, a4, command);
    }

    private delegate nint ReceiveRetainerVentureListUpdateDelegate(nint a1, int a2, nint a3);
    [Signature("40 53 41 55 41 56 41 57 48 83 EC 28 8B DA", DetourName = nameof(ReceiveRetainerVentureListUpdateDetour), Fallibility = Fallibility.Infallible)]
    private Hook<ReceiveRetainerVentureListUpdateDelegate> ReceiveRetainerVentureListUpdateHook;

    private nint ReceiveRetainerVentureListUpdateDetour(nint a1, int a2, nint a3)
    {
        var ret = ReceiveRetainerVentureListUpdateHook.Original(a1, a2, a3);
        DebugLog($"{a1:X16}, {a2:X8}, {a3:X16}");
        P.ListUpdateFrame = CSFramework.Instance()->FrameCounter;
        return ret;
    }

    private nint AddonItemSearchResult_OnRequestedUpdateDelegateDetour(nint a1, nint data)
    {
        var ret = OnReceiveMarketPricePacketHook.Original(a1, data);
        P.MarketCooldownOverlay.UnlockAt = Environment.TickCount64 + 2000;
        return ret;
    }

    public void Dispose()
    {
        InteractWithObjectHook?.Dispose();
        OnReceiveMarketPricePacketHook?.Dispose();
        ReceiveRetainerVentureListUpdateHook?.Dispose();
    }

    internal void AddonAirShipExploration_SelectDestinationDetour(nint a1, nint a2, AirshipExplorationInputData* a3)
    {
        ((AtkUnitBase*)a1)->ReceiveEvent((AtkEventType)35, 0, (AtkEvent*)a2, (AtkEventData*)a3);
    }

    internal void SelectRoutePointUnsafe(int which)
    {
        if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
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

    private delegate void SellItemDelegate(uint a1, InventoryType a2, uint a3);
    [EzHook("48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B F2 8B E9", false)]
    private EzHook<SellItemDelegate> SellItemHook;

    private void SellItemDetour(uint inventorySlot, InventoryType a2, uint a3)
    {
        DebugLog($"SellItemDetour: {inventorySlot}, {a2}, {a3}");
        SellItemHook.Original(inventorySlot, a2, a3);
    }

    public void SellItemToShop(InventoryType type, int slot)
    {
        if(TryGetAddonByName<AtkUnitBase>("Shop", out var addon) && IsAddonReady(addon))
        {
            var slotPtr = InventoryManager.Instance()->GetInventoryContainer(type)->GetInventorySlot(slot);
            if(slotPtr->ItemId != 0)
            {
                if(Data.GetIMSettings().IMProtectList.Contains(slotPtr->ItemId)) throw new InvalidOperationException($"Attempted to sell protected item: {ExcelItemHelper.GetName(slotPtr->ItemId)}");
                SellItemDetour((uint)slot, type, 0);
            }
            else
            {
                PluginLog.Warning($"Requested inventory slot {type}({slot}) had no item in it to sell.");
            }
        }
        else
        {
            throw new InvalidOperationException("Could not find Shop.");
        }
    }
}
