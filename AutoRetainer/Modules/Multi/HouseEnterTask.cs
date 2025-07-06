using ECommons.Automation;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace AutoRetainer.Modules.Multi;

internal static unsafe class HouseEnterTask
{
    internal static bool? Approach()
    {
        DebugLog($"Enabling automove");
        Utils.RegenerateRandom();
        Chat.ExecuteCommand("/automove on");
        return true;
    }

    internal static bool? SelectYesno()
    {
        if(!ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType))
        {
            return null;
        }
        var addon = Utils.GetSpecificYesno(Lang.ConfirmHouseEntrance);
        if(addon != null)
        {
            if(IsAddonReady(addon) && EzThrottler.Throttle("HET.SelectYesno"))
            {
                DebugLog("Select yes");
                new AddonMaster.SelectYesno((nint)addon).Yes();
                return true;
            }
        }
        else
        {
            if(Utils.TrySelectSpecificEntry(Lang.GoToYourApartment, () => EzThrottler.Throttle("HET.SelectYesno")))
            {
                DebugLog("Confirmed going to apartment");
                return true;
            }
        }
        return false;
    }

    internal static bool? WaitUntilLeavingZone()
    {
        return !ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType);
    }

    internal static bool? LockonBell()
    {
        var bell = Utils.GetNearestRetainerBell(out var d);
        if(bell != null && d < 20f)
        {
            if(Svc.Targets.Target?.Address == bell.Address)
            {
                if(EzThrottler.Throttle("HET.LockonBell"))
                {
                    Chat.ExecuteCommand("/lockon");
                    return true;
                }
            }
            else
            {
                if(EzThrottler.Throttle("HET.SetTargetBell", 200))
                {
                    DebugLog($"Setting bell target ({bell})");
                    Svc.Targets.Target = bell;
                }
            }
        }
        return false;
    }


    internal static bool? AutorunOffBell()
    {
        var bell = Utils.GetReachableRetainerBell(false);
        if(bell != null) PluginLog.Information($"Dist {Vector3.Distance(Player.Object.Position, bell.Position)}");
        if(bell != null && Vector3.Distance(Player.Object.Position, bell.Position) < 4f + Utils.Random * 0.25f)
        {
            DebugLog($"Disabling automove");
            Chat.ExecuteCommand("/automove off");
            return true;
        }
        return false;
    }
}
