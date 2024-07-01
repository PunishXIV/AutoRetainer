using ECommons.Automation;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskEnterWorkshop
{
    internal static void Enqueue()
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskEnterWorkshop)}");
        P.TaskManager.Enqueue(() => !IsOccupied(), 180 * 1000, "WaitUntilNotOccupied1");
        P.TaskManager.Enqueue(() =>
        {
            if (VoyageUtils.ShouldEnterWorkshop())
            {
                if (Utils.GetNearestWorkshopEntrance(out _) != null)
                {
                    EnqueueImmediateEnterWorkshop();
                }
            }
        });
    }

    internal static void EnqueueImmediateEnterWorkshop()
    {
        P.TaskManager.EnqueueImmediate(() => !IsOccupied(), 180 * 1000, "WaitUntilNotOccupied2");
        P.TaskManager.EnqueueImmediate(LockonAdditionalChambers, 1000, true);
        P.TaskManager.EnqueueImmediate(HouseEnterTask.Approach);
        P.TaskManager.EnqueueImmediate(AutorunOffAdd);
        P.TaskManager.EnqueueImmediate(() => { Chat.Instance.ExecuteCommand("/automove off"); });
        P.TaskManager.EnqueueImmediate(InteractAdd);
        P.TaskManager.EnqueueImmediate(SelectEnterWorkshop);
        P.TaskManager.EnqueueImmediate(() => VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType), "Wait Until entered workshop");
        P.TaskManager.DelayNextImmediate(60, true);
    }

    internal static void EnqueueEnterWorkshop()
    {
        P.TaskManager.Enqueue(() => !IsOccupied(), 180 * 1000, "WaitUntilNotOccupied2");
        P.TaskManager.Enqueue(LockonAdditionalChambers, 1000, true);
        P.TaskManager.Enqueue(HouseEnterTask.Approach);
        P.TaskManager.Enqueue(AutorunOffAdd);
        P.TaskManager.Enqueue(() => { Chat.Instance.ExecuteCommand("/automove off"); });
        P.TaskManager.Enqueue(InteractAdd);
        P.TaskManager.Enqueue(SelectEnterWorkshop);
        P.TaskManager.Enqueue(() => VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType), "Wait Until entered workshop");
        P.TaskManager.DelayNext(60, true);
    }

    internal static bool? SelectEnterWorkshop()
    {
        if (Utils.TrySelectSpecificEntry(Lang.EnterWorkshop, () => EzThrottler.Throttle("HET.SelectEnterWorkshop")))
        {
            DebugLog("Confirmed going to workhop");
            return true;
        }
        return false;
    }

    internal static bool? InteractAdd()
    {
        var entrance = Utils.GetNearestWorkshopEntrance(out var d);
        if (entrance != null && Svc.Targets.Target?.Address == entrance.Address && EzThrottler.Throttle("HET.InteractAdd", 1000))
        {
            DebugLog($"Interacting with entrance");
            TargetSystem.Instance()->InteractWithObject((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)entrance.Address, false);
            return true;
        }
        return false;
    }
    internal static bool? AutorunOffAdd()
    {
        var entrance = Utils.GetNearestWorkshopEntrance(out var d);
        if (entrance != null && d < 3f + Utils.Random && EzThrottler.Throttle("HET.DisableAutomoveAdd"))
        {
            DebugLog($"Disabling automove");
            Chat.Instance.ExecuteCommand("/automove off");
            return true;
        }
        return false;
    }

    internal static bool? LockonAdditionalChambers()
    {
        var entrance = Utils.GetNearestWorkshopEntrance(out _);
        if (entrance != null)
        {
            if (Svc.Targets.Target?.Address == entrance.Address)
            {
                if (EzThrottler.Throttle("HET.LockonAdd"))
                {
                    Chat.Instance.ExecuteCommand("/lockon");
                    return true;
                }
            }
            else
            {
                if (EzThrottler.Throttle("HET.SetTargetAdd", 200))
                {
                    DebugLog($"Setting entrance target ({entrance})");
                    Svc.Targets.Target = entrance;
                }
            }
        }
        return false;
    }
}
