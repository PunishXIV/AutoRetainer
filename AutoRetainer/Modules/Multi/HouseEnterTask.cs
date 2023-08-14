using AutoRetainer.Modules.Voyage;
using ClickLib.Clicks;
using ECommons.Automation;
using ECommons.Events;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AutoRetainer.Modules.Multi;

internal unsafe static class HouseEnterTask
{
    internal static void EnqueueTask()
    {
        P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
        P.TaskManager.Enqueue(WaitUntilNotBusy, 180 * 1000);
        P.TaskManager.Enqueue(() =>
        {
            if(Utils.GetReachableRetainerBell() == null)
            {
                var entrance = Utils.GetNearestEntrance(out var d);
                var validDistance = entrance.IsApartmentEntrance() ? 4.85f : 4f;
                if (entrance != null && d > validDistance)
                {
                    P.TaskManager.EnqueueImmediate(() => SetTarget(40f));
                    P.TaskManager.EnqueueImmediate(Lockon);
                    P.TaskManager.EnqueueImmediate(Approach);
                    P.TaskManager.EnqueueImmediate(AutorunOff);
                    P.TaskManager.EnqueueImmediate(() => { Chat.Instance.SendMessage("/automove off"); });
                }
                P.TaskManager.EnqueueImmediate(() => SetTarget(5f));
                P.TaskManager.EnqueueImmediate(Interact);
                P.TaskManager.EnqueueImmediate(SelectYesno);
                P.TaskManager.EnqueueImmediate(WaitUntilLeavingZone);
                if (C.EnterWorkshop)
                {
                    P.TaskManager.EnqueueImmediate(() => !IsOccupied(), 180 * 1000, "WaitUntilNotOccupied");
                    P.TaskManager.EnqueueImmediate(LockonAdditionalChambers, 1000, true);
                    P.TaskManager.EnqueueImmediate(Approach);
                    P.TaskManager.EnqueueImmediate(AutorunOffAdd);
                    P.TaskManager.EnqueueImmediate(() => { Chat.Instance.SendMessage("/automove off"); });
                    P.TaskManager.EnqueueImmediate(InteractAdd);
                    P.TaskManager.EnqueueImmediate(SelectEnterWorkshop);
                    P.TaskManager.EnqueueImmediate(() => VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType), "Wait Until entered workshop");
                }
            }
            return true;
        });
    }

    internal static bool? WaitUntilNotBusy()
    {
        if (!ProperOnLogin.PlayerPresent || !ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType)) return null;
        if (MultiMode.IsInteractionAllowed())
        {
            return true;
        }
        return false;
    }

    internal static bool? Lockon()
    {
        var entrance = Utils.GetNearestEntrance(out _);
        if (entrance != null && Svc.Targets.Target?.Address == entrance.Address && EzThrottler.Throttle("HET.Lockon"))
        {
            Chat.Instance.SendMessage("/lockon");
            return true;
        }
        return false;
    }

    internal static bool? Approach()
    {
        P.DebugLog($"Enabling automove");
        Chat.Instance.SendMessage("/automove on");
        return true;
    }

    internal static bool? AutorunOff()
    {
        var entrance = Utils.GetNearestEntrance(out var d);
        if (entrance != null && d < 4f && EzThrottler.Throttle("HET.DisableAutomove"))
        {
            P.DebugLog($"Disabling automove");
            Chat.Instance.SendMessage("/automove off");
            return true;
        }
        return false;
    }

    internal static bool? SetTarget(float distance)
    {
        var entrance = Utils.GetNearestEntrance(out var d);
        if (entrance != null && d < distance && EzThrottler.Throttle("HET.SetTarget", 200))
        {
            P.DebugLog($"Setting entrance target ({distance})");
            Svc.Targets.Target = (entrance);
            return true;
        }
        return false;
    }

    internal static bool? Interact()
    {
        var entrance = Utils.GetNearestEntrance(out var d);
        if (entrance != null && Svc.Targets.Target?.Address == entrance.Address && EzThrottler.Throttle("HET.Interact", 1000))
        {
            P.DebugLog($"Interacting with entrance");
            TargetSystem.Instance()->InteractWithObject((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)entrance.Address, false);
            return true;
        }
        return false;
    }

    internal static bool? InteractAdd()
    {
        var entrance = Utils.GetNearestWorkshopEntrance(out var d);
        if (entrance != null && Svc.Targets.Target?.Address == entrance.Address && EzThrottler.Throttle("HET.InteractAdd", 1000))
        {
            P.DebugLog($"Interacting with entrance");
            TargetSystem.Instance()->InteractWithObject((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)entrance.Address, false);
            return true;
        }
        return false;
    }

    internal static bool? SelectYesno()
    {
        if (!ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType))
        {
            return null;
        }
        var addon = Utils.GetSpecificYesno(Lang.ConfirmHouseEntrance);
        if (addon != null)
        {
            if (IsAddonReady(addon) && EzThrottler.Throttle("HET.SelectYesno"))
            {
                P.DebugLog("Select yes");
                ClickSelectYesNo.Using((nint)addon).Yes();
                return true;
            }
        }
        else
        {
            if (Utils.TrySelectSpecificEntry(Lang.GoToYourApartment, () => EzThrottler.Throttle("HET.SelectYesno")))
            {
                P.DebugLog("Confirmed going to apartment");
                return true;
            }
        }
        return false;
    }

    internal static bool? WaitUntilLeavingZone()
    {
        return !ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType);
    }
    
    internal static bool? LockonAdditionalChambers()
    {
        var entrance = Utils.GetNearestWorkshopEntrance(out _);
        if (entrance != null)
        {
            if(Svc.Targets.Target?.Address == entrance.Address)
            {
                if (EzThrottler.Throttle("HET.LockonAdd"))
                {
                    Chat.Instance.SendMessage("/lockon");
                    return true;
                }
            }
            else
            {
                if(EzThrottler.Throttle("HET.SetTargetAdd", 200))
                {
                    P.DebugLog($"Setting entrance target ({entrance})");
                    Svc.Targets.Target = entrance;
                }
            }
        }
        return false;
    }

    internal static bool? AutorunOffAdd()
    {
        var entrance = Utils.GetNearestWorkshopEntrance(out var d);
        if (entrance != null && d < 4f && EzThrottler.Throttle("HET.DisableAutomoveAdd"))
        {
            P.DebugLog($"Disabling automove");
            Chat.Instance.SendMessage("/automove off");
            return true;
        }
        return false;
    }

    internal static bool? SelectEnterWorkshop()
    {
        if (Utils.TrySelectSpecificEntry("Move to the company workshop", () => EzThrottler.Throttle("HET.SelectEnterWorkshop")))
        {
            P.DebugLog("Confirmed going to apartment");
            return true;
        }
        return false;
    }
}
