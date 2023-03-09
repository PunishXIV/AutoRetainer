using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.GameFunctions;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace AutoRetainer.NewScheduler.Handlers
{
    internal unsafe static class PlayerWorldHandlers
    {
        internal static bool? SelectNearestBell()
        {
            if (!IsOccupied())
            {
                var x = Utils.GetReachableRetainerBell();
                if(x != null)
                {
                    if (Utils.GenericThrottle)
                    {
                        Svc.Targets.SetTarget(x);
                        PluginLog.Debug($"Set target to {x}");
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool? InteractWithTargetedBell()
        {
            var x = Svc.Targets.Target;
            if (x != null && (x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(Consts.BellName, "リテイナーベル") && !IsOccupied())
            {
                if (Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < Utils.GetValidInteractionDistance(x) && x.IsTargetable())
                {
                    if (Utils.GenericThrottle && EzThrottler.Throttle("InteractWithBell", 5000))
                    {
                        TargetSystem.Instance()->InteractWithObject((GameObject*)x.Address, false);
                        PluginLog.Debug($"Interacted with {x}");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
