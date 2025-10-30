using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Internal.InventoryManagement;
public static unsafe class NpcSaleManager
{
    internal static List<(uint ID, uint Quantity)> CapturedInventoryState = [];
    public static void EnqueueIfItemsPresent()
    {
        EnqueueIfItemsPresent(false);
    }

    public static void EnqueueIfItemsPresent(bool ignoreRestriction)
    {
        if(Utils.ShouldSkipNPCVendor() && !ignoreRestriction) return;
        if(GetValidNPC() == null) return;
        if(!Data.GetIMSettings().IMEnableNpcSell) return;
        foreach(var type in InventorySpaceManager.GetAllowedToSellInventoryTypes())
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(type);
            if(inv != null)
            {
                for(var i = 0; i < inv->Size; i++)
                {
                    var slot = inv->GetInventorySlot(i);
                    if(slot != null && slot->ItemId != 0)
                    {
                        if(Utils.IsItemSellableByHardList(slot->ItemId, slot->Quantity))
                        {
                            P.TaskManager.BeginStack();
                            P.TaskManager.Enqueue(Utils.WaitForScreen);
                            P.TaskManager.Enqueue(InteractWithNPC);
                            P.TaskManager.Enqueue(SelectPurchase);
                            P.TaskManager.EnqueueDelay(500);
                            P.TaskManager.Enqueue(SellHardListItemsTask, new(timeLimitMS: 1000 * 60 * 5));
                            P.TaskManager.Enqueue(CloseShop);
                            P.TaskManager.InsertStack();
                            return;
                        }
                    }
                }
            }
        }
    }

    public static bool? SellHardListItemsTask()
    {

        if(!EzThrottler.Check("NpcInventoryTimeout") && Utils.GetCapturedInventoryState(InventorySpaceManager.GetAllowedToSellInventoryTypes()).SequenceEqual(CapturedInventoryState))
        {
            return false;
        }
        List<(InventoryType Type, int Slot)> Processed = [];
        foreach(var type in InventorySpaceManager.GetAllowedToSellInventoryTypes())
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(type);
            if(inv != null)
            {
                for(var i = 0; i < inv->Size; i++)
                {
                    var slot = inv->GetInventorySlot(i);
                    if(!Processed.Contains((type, i)) && slot != null && slot->ItemId != 0)
                    {
                        if(Utils.IsItemSellableByHardList(slot->ItemId, slot->Quantity))
                        {
                            if(EzThrottler.Throttle("VendorItem", 500))
                            {
                                Processed.Add((type, i));
                                EzThrottler.Throttle("NpcInventoryTimeout", 5000, true);
                                P.Memory.SellItemToShop(type, i);
                            }
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    public static uint[] VendorDataIDThroughPreHandler
    {
        get
        {
            field ??= Svc.Data.GetExcelSheet<ENpcBase>().Where(cls => cls.ENpcData.Any(data => data.Is<PreHandler>() && data.TryGetValue(out PreHandler preHandler) && preHandler.Target.Is<GilShop>())).Select(x => x.RowId).ToArray();
            return field;
        }
    }

    public static uint[] VendorDataIDGilShop
    {
        get
        {
            field ??= Svc.Data.GetExcelSheet<ENpcBase>().Where(cls => cls.ENpcData.Any(data => data.Is<GilShop>())).Select(x => x.RowId).ToArray();
            return field;
        }
    }

    public static IGameObject GetValidNPC()
    {
        return Svc.Objects.OrderBy(x => Vector3.Distance(Player.Position, x.Position))
            .Where(x =>
            x.DataId.EqualsAny(VendorDataIDGilShop)
            && x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.EventNpc
            && Vector3.Distance(Player.Position, x.Position) < 7f
            ).FirstOrDefault();
    }

    public static bool? InteractWithNPC()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectIconString", out _)) return true;
        var npc = GetValidNPC() ?? throw new InvalidOperationException("Could not find housing NPC");
        if(Svc.Targets.Target?.Address != npc.Address)
        {
            if(EzThrottler.Throttle("TargetNPC"))
            {
                Svc.Targets.Target = npc;
            }
            return false;
        }
        else
        {
            if(Player.IsAnimationLocked) return false;
            if(EzThrottler.Throttle("InteractWithNPC", 2000))
            {
                TargetSystem.Instance()->InteractWithObject(npc.Struct(), false);
            }
            return false;
        }
    }

    public static bool? SelectPurchase()
    {
        if(TryGetAddonByName<AtkUnitBase>("Shop", out var addon) && IsAddonReady(addon)) return true;
        if(TryGetAddonMaster<AddonMaster.SelectIconString>(out var m))
        {
            foreach(var entry in m.Entries)
            {
                if(Svc.Data.GetExcelSheet<GilShop>().Select(x => x.Name.GetText()).Contains(entry.Text))
                {
                    if(EzThrottler.Throttle("SelectStringSell", 2000))
                    {
                        entry.Select();
                    }
                    return false;
                }
            }
        }
        return false;
    }

    public static bool? CloseShop()
    {
        if(TryGetAddonByName<AtkUnitBase>("Shop", out var addon) && IsAddonReady(addon))
        {
            if(EzThrottler.Throttle("CloseShop", 2000))
            {
                Callback.Fire(addon, true, -1);
            }
            return false;
        }
        else
        {
            return true;
        }
    }
}
