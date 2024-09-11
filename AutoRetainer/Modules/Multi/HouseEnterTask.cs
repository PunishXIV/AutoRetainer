using AutoRetainer.Modules.Voyage.Tasks;
using ECommons.Automation;
using ECommons.Events;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Diagnostics;

namespace AutoRetainer.Modules.Multi;

internal static unsafe class HouseEnterTask
{
    private static void EnqueueTask()
    {
        PluginLog.Debug($"Enqueued HouseEnterTask from {new StackTrace().GetFrames().Select(x => x.GetMethod()?.Name).Prepend("      ").Print("\n")}");
        P.TaskManager.Enqueue(NewYesAlreadyManager.WaitForYesAlreadyDisabledTask);
        P.TaskManager.Enqueue(WaitUntilNotBusy, new(timeLimitMS:180 * 1000));
        P.TaskManager.Enqueue(() =>
        {
            P.TaskManager.BeginStack();
            try
            {
                if(Utils.GetReachableRetainerBell(false) == null)
                {
                    var entrance = Utils.GetNearestEntrance(out var d);
                    var validDistance = 4f;
                    if(entrance != null && d > validDistance)
                    {
                        P.TaskManager.Enqueue(() => SetTarget(40f));
                        P.TaskManager.Enqueue(Lockon);
                        P.TaskManager.Enqueue(Approach);
                        P.TaskManager.Enqueue(AutorunOff);
                        P.TaskManager.Enqueue(() => { Chat.Instance.ExecuteCommand("/automove off"); });
                    }
                    else if(entrance == null)
                    {
                        return null;
                    }
                    P.TaskManager.Enqueue(() => SetTarget(5f));
                    P.TaskManager.Enqueue(Interact);
                    P.TaskManager.Enqueue(SelectYesno);
                    P.TaskManager.Enqueue(WaitUntilLeavingZone);
                    P.TaskManager.EnqueueDelay(60, true);
                    P.TaskManager.Enqueue(Utils.WaitForScreen);
                }
            }
            catch(Exception e) { e.Log(); }
            P.TaskManager.InsertStack();
            return true;
        }, "Master HET");
        //TaskContinueHET.Enqueue();
    }

    private static bool? WaitUntilNotBusy()
    {
        if(!IsScreenReady()) return false;
        if(!ProperOnLogin.PlayerPresent || !ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType)) return null;
        if(MultiMode.IsInteractionAllowed())
        {
            return true;
        }
        return false;
    }

    private static bool? Lockon()
    {
        var entrance = Utils.GetNearestEntrance(out _);
        if(entrance != null && Svc.Targets.Target?.Address == entrance.Address && EzThrottler.Throttle("HET.Lockon"))
        {
            Chat.Instance.ExecuteCommand("/lockon");
            return true;
        }
        return false;
    }

    internal static bool? Approach()
    {
        DebugLog($"Enabling automove");
        Utils.RegenerateRandom();
        Chat.Instance.ExecuteCommand("/automove on");
        return true;
    }

    internal static bool? AutorunOff()
    {
        var entrance = Utils.GetNearestEntrance(out var d);
        if(entrance != null && d < 3f + Utils.Random && EzThrottler.Throttle("HET.DisableAutomove"))
        {
            DebugLog($"Disabling automove");
            Chat.Instance.ExecuteCommand("/automove off");
            return true;
        }
        return false;
    }


    internal static bool? SetTarget(float distance)
    {
        var entrance = Utils.GetNearestEntrance(out var d);
        if(entrance != null && d < distance && EzThrottler.Throttle("HET.SetTarget", 200))
        {
            DebugLog($"Setting entrance target ({distance})");
            Svc.Targets.Target = (entrance);
            return true;
        }
        return false;
    }

    internal static bool? Interact()
    {
        var entrance = Utils.GetNearestEntrance(out var d);
        if(entrance != null && Svc.Targets.Target?.Address == entrance.Address && EzThrottler.Throttle("HET.Interact", 1000))
        {
            DebugLog($"Interacting with entrance");
            TargetSystem.Instance()->InteractWithObject((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)entrance.Address, false);
            return true;
        }
        return false;
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
                    Chat.Instance.ExecuteCommand("/lockon");
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
            Chat.Instance.ExecuteCommand("/automove off");
            return true;
        }
        return false;
    }
}
