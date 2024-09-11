using AutoRetainer.Internal.InventoryManagement;
using ECommons.Automation;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskContinueHET
{
    internal static bool? SelectEnterWorkshop()
    {
        if(Utils.TrySelectSpecificEntry(Lang.EnterWorkshop, () => EzThrottler.Throttle("HET.SelectEnterWorkshop")))
        {
            DebugLog("Confirmed going to workhop");
            return true;
        }
        return false;
    }
}
