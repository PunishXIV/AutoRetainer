using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Internal.InventoryManagement;
public static unsafe class NpcSaleManager
{
    internal static List<(uint ID, uint Quantity)> CapturedInventoryState = [];
    public static void EnqueueIfItemsPresent()
    {
        if(GetValidNPC() == null) return;
        if(!C.IMEnableNpcSell) return;
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
                            P.TaskManager.EnqueueImmediate(Utils.WaitForScreen);
                            P.TaskManager.EnqueueImmediate(InteractWithNPC);
                            P.TaskManager.EnqueueImmediate(SelectPurchase);
                            P.TaskManager.DelayNextImmediate(500);
                            P.TaskManager.EnqueueImmediate(SellHardListItemsTask, 1000 * 60 * 5);
                            P.TaskManager.EnqueueImmediate(CloseShop);
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

    public static uint[] VendorDataID = [1008837, 1008838, 1008839, 1008840, 1008841, 1008842, 1008843, 1008844, 1008845, 1008846, 1013117, 1013118, 1008847, 1008848, 1008849, 1008850, 1008855, 1008854, 1008853, 1008852, 1008851, 1008856, 1013119, 1013120, 1016176, 1016177, 1016178, 1016179, 1016180, 1016181, 1016182, 1016183, 1016184, 1016185, 1016186, 1016187, 1018662, 1018663, 1018664, 1018665, 1018666, 1018667, 1018668, 1018669, 1018670, 1018671, 1018672, 1018673, 1018675, 1018674, 1018676, 1018677, 1018678, 1018679, 1018680, 1018681, 1018682, 1018683, 1018684, 1018685, 1024549, 1024548, 1024550, 1024551, 1024552, 1024553, 1024554, 1024555, 1024556, 1024557, 1024558, 1024559, 1024560, 1024561, 1024562, 1024563, 1024564, 1024565, 1024566, 1024567, 1024568, 1024569, 1024570, 1024571, 1025027, 1025028, 1025029, 1025030, 1025031, 1025032, 1025033, 1025034, 1025035, 1025036, 1025037, 1025038, 1025039, 1025040, 1025042, 1025043, 1025044, 1025046, 1025717, 1026169, 1025718, 1026170, 1025720, 1026172, 1025913, 1025914, 1025915, 1025916, 1025917, 1025918, 1025922, 1025923, 1025924, 1025921, 1025920, 1025919, 1027015, 1027016, 1027018, 1045242, 1045256, 1045257, 1045243, 1045245, 1045259];

    public static IGameObject GetValidNPC()
    {
        return Svc.Objects.OrderBy(x => Vector3.Distance(Player.Position, x.Position))
            .Where(x =>
            x.DataId.EqualsAny(VendorDataID)
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
                if(Svc.Data.GetExcelSheet<GilShop>().Select(x => x.Name.ExtractText()).Contains(entry.Text))
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
