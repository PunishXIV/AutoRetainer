using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Tasks;
using ECommons.UIHelpers.AddonMasterImplementations;
using ECommons.UIHelpers.AtkReaderImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Scheduler.Handlers;

internal static unsafe class RetainerListHandlers
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
            var list = new AddonMaster.RetainerList(retainerList);
            foreach(var retainer in list.Retainers)
            {
                if (retainer.Name == name)
                {
                    if (Utils.GenericThrottle)
                    {
                        DebugLog($"Selecting retainer {retainer.Name} with index {retainer.Index}");
                        retainer.Select();
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
