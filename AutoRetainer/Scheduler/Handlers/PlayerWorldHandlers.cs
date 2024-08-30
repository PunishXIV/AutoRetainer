using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace AutoRetainer.Scheduler.Handlers;

internal static unsafe class PlayerWorldHandlers
{
    internal static bool? SelectNearestBell()
    {
        if(!IsOccupied())
        {
            var x = Utils.GetReachableRetainerBell(false);
            if(x != null)
            {
                if(Utils.GenericThrottle)
                {
                    Svc.Targets.SetTarget(x);
                    DebugLog($"Set target to {x}");
                    return true;
                }
            }
        }
        return false;
    }

    internal static bool? InteractWithTargetedBell()
    {
        var x = Svc.Targets.Target;
        if(x != null && (x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Lang.BellName) && !IsOccupied())
        {
            if(Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < Utils.GetValidInteractionDistance(x) && x.IsTargetable)
            {
                if(Utils.GenericThrottle && EzThrottler.Throttle("InteractWithBell", 5000))
                {
                    TargetSystem.Instance()->InteractWithObject((GameObject*)x.Address, false);
                    DebugLog($"Interacted with {x}");
                    return true;
                }
            }
        }
        return false;
    }
}
