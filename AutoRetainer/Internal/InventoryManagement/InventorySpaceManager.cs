using AutoRetainer.Scheduler.Tasks;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Internal.InventoryManagement;

public static unsafe class InventorySpaceManager
{
    public static readonly List<string> Log = [];
    public static readonly string[] Addons = ["InventoryRetainer", "InventoryRetainerLarge"];

    public static nint AgentRetainerItemCommandModule => (nint)AgentModule.Instance()->GetAgentByInternalId(AgentId.Retainer) + 40;

    private static bool IsAgentRetainerActive => AgentModule.Instance()->GetAgentByInternalId(AgentId.Retainer)->IsAgentActive();

    public static readonly List<SellSlotTask> SellSlotTasks = [];

    public static InventoryType[] GetAllowedToSellInventoryTypes() => C.AllowSellFromArmory?[..Utils.PlayerInvetories, ..Utils.PlayerArmory]:Utils.PlayerInvetories;

    public static bool? SafeSellSlot(SellSlotTask Task)
    {
        if(Utils.GenericThrottle && EzThrottler.Throttle("SellSlot", 333))
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(Task.InventoryType);
            if(inv == null)
            {
                DuoLog.Warning($"Inventory {Task.InventoryType} is null");
                return true;
            }
            if(C.IMProtectList.Contains(Task.ItemID))
            {
                DuoLog.Warning($"Item {Task} is protected and won't be sold.");
                return true;
            }
            var slot = inv->Items[Task.Slot];
            if(Task.ItemID != slot.ItemId || slot.ItemId == 0 || slot.Quantity != Task.Quantity)
            {
                DuoLog.Warning($"Slot contains different item {ExcelItemHelper.GetName(slot.ItemId)}x{slot.Quantity}, should be {Task}");
                return true;
            }
            if(!IsRetainerInventoryLoaded())
            {
                DuoLog.Warning($"Could not find retainer inventory");
                return true;
            }
            if(!IsAgentRetainerActive)
            {
                DuoLog.Warning($"AgentRetainer is not active");
                return true;
            }
            if(!C.IMDry)
            {
                P.Memory.RetainerItemCommandDetour(AgentRetainerItemCommandModule, Task.Slot, Task.InventoryType, 0, RetainerItemCommand.HaveRetainerSellItem);
                PluginLog.Debug($"Sold slot {Task}");
            }
            else
            {
                DuoLog.Warning($"> IMDry > Would sell slot {Task}");
            }
            Log.Add($"[{DateTime.Now}] Sold {Task} on {Data.Name}");
            return true;
        }
        return false;
    }

    public static bool IsRetainerInventoryLoaded()
    {
        foreach(var addonCheck in Addons)
        {
            if(TryGetAddonByName<AtkUnitBase>(addonCheck, out var addon) && IsAddonReady(addon))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsSlotEnqueued(InventoryType type, uint slot)
    {
        return SellSlotTasks.Any(x => x.InventoryType == type && x.Slot == slot);
    }

    public static void EnqueueSoftItemIfAllowed(uint ItemId, uint Quantity)
    {
        var im = InventoryManager.Instance();
        foreach(var invType in InventorySpaceManager.GetAllowedToSellInventoryTypes())
        {
            var inv = im->GetInventoryContainer(invType);
            for(var i = 0; i < inv->Size; i++)
            {
                var item = inv->Items[i];
                if(item.ItemId != 0 && item.ItemId == ItemId && item.Quantity == Quantity)
                {
                    if(C.IMAutoVendorSoft.Contains(item.ItemId))
                    {
                        var task = new SellSlotTask(invType, (uint)i, item.ItemId, item.Quantity);
                        PluginLog.Information($"Enqueueing {task} for soft sale");
                        InventorySpaceManager.SellSlotTasks.Add(task);
                        return;
                    }
                }
            }
        }
    }

    public static void EnqueueAllHardItems()
    {
        var im = InventoryManager.Instance();
        foreach(var invType in InventorySpaceManager.GetAllowedToSellInventoryTypes())
        {
            var inv = im->GetInventoryContainer(invType);
            for(var i = 0; i < inv->Size; i++)
            {
                var item = inv->Items[i];
                if(item.ItemId != 0 && (item.Quantity < C.IMAutoVendorHardStackLimit || C.IMAutoVendorHardIgnoreStack.Contains(item.ItemId)))
                {
                    if(C.IMAutoVendorHard.Contains(item.ItemId) && !TaskDesynthItems.DesynthEligible(item.ItemId))
                    {
                        var task = new SellSlotTask(invType, (uint)i, item.ItemId, item.Quantity);
                        PluginLog.Information($"Enqueueing {task} for hard sale");
                        InventorySpaceManager.SellSlotTasks.Add(task);
                    }
                }
            }
        }
    }
}
