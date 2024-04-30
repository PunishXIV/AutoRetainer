using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Tasks;
using ClickLib.Clicks;
using ECommons.UIHelpers.Implementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Scheduler.Handlers;

internal unsafe static class RetainerListHandlers
{
    internal static bool? SelectRetainerByName(string name)
    {
        TaskWithdrawGil.forceCheck = false;
        InventorySpaceManager.SellSlotTasks.Clear();
        if (name.IsNullOrEmpty())
        {
            throw new Exception($"Name can not be null or empty");
        }
        if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
        {
            var list = new ReaderRetainerList(retainerList);
            for (int i = 0; i < list.Retainers.Count; i++)
            {
                if (list.Retainers[i].Name == name)
                {
                    if (Utils.GenericThrottle)
                    {
                        DebugLog($"Selecting retainer {list.Retainers[i].Name} with index {i}");
                        ClickRetainerList.Using((IntPtr)retainerList).Retainer(i);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    internal static bool? CloseRetainerList()
    {
        if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
        {
            if (Utils.GenericThrottle)
            {
                var v = stackalloc AtkValue[1]
                {
                    new()
                    {
                        Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                        Int = -1
                    }
                };
                P.IsCloseActionAutomatic = true;
                retainerList->FireCallback(1, v);
                DebugLog($"Closing retainer window");
                return true;
            }
        }
        return false;
    }
}
