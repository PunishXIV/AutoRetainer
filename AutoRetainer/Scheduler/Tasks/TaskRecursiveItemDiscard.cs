using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;

namespace AutoRetainer.Scheduler.Tasks;
public unsafe static class TaskRecursiveItemDiscard
{
    private static List<InventoryDescriptor> ProcessedSlots = [];
    public static void EnqueueIfNeeded()
    {
        if(Data.GetIMSettings().IMDiscardList.Count == 0) return;
        P.TaskManager.Enqueue(() =>
        {
            ProcessedSlots.Clear();
            if(Utils.InventoryContainsDiscardableItems())
            {
                P.TaskManager.Insert(RecursivelyDiscardItems);
            }
        }, $"{nameof(TaskRecursiveItemDiscard)} master task");
    }

    private static bool RecursivelyDiscardItems()
    {
        var im = InventoryManager.Instance();
        foreach(var invType in Data.GetDiscardableInventories())
        {
            var cont = im->GetInventoryContainer(invType);
            for(int i = 0; i < cont->Size; i++)
            {
                var slot = cont->Items[i];
                if(ProcessedSlots.Contains(new(invType, i)) || Data.GetIMSettings().IMProtectList.Contains(slot.ItemId))
                {
                    continue;
                }
                else if(Data.GetIMSettings().IMDiscardList.Contains(slot.ItemId)
                    && (slot.Quantity < Data.GetIMSettings().IMDiscardStackLimit || Data.GetIMSettings().IMDiscardIgnoreStack.Contains(slot.ItemId)))
                {
                    if(EzThrottler.Check("DiscardItem") && Utils.GenericThrottle && EzThrottler.Throttle("DiscardItem", Random.Shared.Next(300, 400)))
                    {
                        ProcessedSlots.Add(new(invType, i));
                        Utils.ExecuteDiscardSafely(invType, i, slot.ItemId);
                        P.TaskManager.InsertMulti(
                            new(WaitUntilConfirmDiscardExists, new(abortOnTimeout:false, timeLimitMS:5000)),
                            new(ConfirmDiscard, new(abortOnTimeout: false, timeLimitMS: 5000)),
                            new(RecursivelyDiscardItems)
                            );
                        return true;
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private static bool WaitUntilConfirmDiscardExists()
    {
        if(Data.GetIMSettings().IMDry)
        {
            return true;
        }
        else
        {
            var addon = Utils.GetSpecificYesno(Lang.DiscardItem);
            return addon != null && addon->IsReady();
        }
    }

    private static bool ConfirmDiscard()
    {
        if(Data.GetIMSettings().IMDry)
        {
            return true;
        }
        else
        {
            var addon = Utils.GetSpecificYesno(Lang.DiscardItem);
            if(addon != null && addon->IsReady())
            {
                var m = new AddonMaster.SelectYesno(addon);
                if(EzThrottler.Throttle("ConfirmDiscard"))
                {
                    m.Yes();
                    return false;
                }
            }
            else if(addon == null)
            {
                return true;
            }
        }
        return false;
    }

    private readonly record struct InventoryDescriptor
    {
        public readonly InventoryType Type;
        public readonly int Slot;

        public InventoryDescriptor(InventoryType type, int slot)
        {
            Type = type;
            Slot = slot;
        }
    }
}