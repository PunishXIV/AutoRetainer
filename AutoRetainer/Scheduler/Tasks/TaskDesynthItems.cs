using Dalamud.Game.ClientState.Conditions;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Scheduler.Tasks;
public static unsafe class TaskDesynthItems
{
    private static readonly InventoryType[] DesynthableInventories =
    [
        InventoryType.Inventory1,
        InventoryType.Inventory2,
        InventoryType.Inventory3,
        InventoryType.Inventory4,
        InventoryType.ArmoryMainHand,
        InventoryType.ArmoryOffHand,
        InventoryType.ArmoryHead,
        InventoryType.ArmoryBody,
        InventoryType.ArmoryHands,
        InventoryType.ArmoryLegs,
        InventoryType.ArmoryFeets,
        InventoryType.ArmoryEar,
        InventoryType.ArmoryNeck,
        InventoryType.ArmoryWrist,
        InventoryType.ArmoryRings
    ];

    public static void Enqueue()
    {
        P.TaskManager.Enqueue(RecursivelyDesynthItems, new(timeLimitMS: 10 * 60 * 1000, abortOnTimeout: false));
        P.TaskManager.Enqueue(() => !Svc.Condition[ConditionFlag.Occupied39]);
    }

    private static bool? RecursivelyDesynthItems()
    {
        if(!QuestManager.IsQuestComplete(65688)) return true;

        var eligibleItems = GetEligibleItems();
        if(eligibleItems.Count == 0) return true;

        foreach(var item in eligibleItems)
        {
            // check IsOccupied vs just Occupied39 or else it might trigger when exiting the retainer menu
            if(Utils.AnimationLock == 0 && !IsOccupied())
            {
                if(Utils.GenericThrottle && EzThrottler.Throttle("DesynthingItem", 1000))
                    DesynthItem(item);
            }
            else
                Utils.RethrottleGeneric();
        }
        return false;
    }

    private static List<Pointer<InventoryItem>> GetEligibleItems()
    {
        List<Pointer<InventoryItem>> items = [];
        foreach(var inv in DesynthableInventories)
        {
            var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
            for(var i = 0; i < cont->Size; ++i)
            {
                var item = cont->GetInventorySlot(i);
                if(IsOnIMList(item->ItemId) && DesynthEligible(item->ItemId))
                    items.Add(item);
            }
        }
        return items;
    }

    public static bool DesynthEligible(uint itemID)
    {
        return Data.GetIMSettings().IMEnableItemDesynthesis
            && ExcelItemHelper.Get(itemID).Value.Desynth > 0
            && PlayerState.Instance()->ClassJobLevels[ExcelItemHelper.Get(itemID).Value.ClassJobRepair.Value.ExpArrayIndex] >= 30;
    }

    private static bool IsOnIMList(uint itemID)
    {
        return !Data.GetIMSettings().IMProtectList.Contains(itemID) && Data.GetIMSettings().IMAutoVendorHard.Contains(itemID);
    }

    private static void DesynthItem(InventoryItem* item)
    {
        Svc.Log.Info($"Desynthing {ExcelItemHelper.GetName(ExcelItemHelper.Get(item->ItemId), true)} [Container={item->Container},Slot={item->Slot}]");
        AgentSalvage.Instance()->SalvageItem(item);
        var retval = new AtkValue();
        Span<AtkValue> param = [
            new AtkValue { Type = ValueType.Int, Int = 0 },
            new AtkValue { Type = ValueType.Bool, Byte = 1 }
        ];
        AgentSalvage.Instance()->AgentInterface.ReceiveEvent(&retval, param.GetPointer(0), 2, 1);
    }
}
