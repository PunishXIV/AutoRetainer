using ECommons.Automation;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler
{
    internal unsafe static class SchedulerVoyage
    {
        internal static bool? Lockon()
        {
            if(VoyageUtils.TryGetNearestVoyagePanel(out var obj))
            {
                if(Svc.Targets.Target?.Address != obj.Address)
                {
                    if (Utils.GenericThrottle)
                    {
                        Svc.Targets.Target = obj;
                    }
                }
                else
                {
                    if (Utils.GenericThrottle)
                    {
                        Chat.Instance.SendMessage("/lockon");
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool? Approach()
        {
            if (VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Svc.Targets.Target?.Address == obj.Address)
            {
                if (Utils.GenericThrottle)
                {
                    Chat.Instance.SendMessage("/automove on");
                    return true;
                }
            }
            return false;
        }

        internal static bool? AutomoveOff()
        {
            if (VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Svc.Targets.Target?.Address == obj.Address)
            {
                if(Vector2.Distance(obj.Position.ToVector2(), Player.Object.Position.ToVector2()) < 2f)
                {
                    if (Utils.GenericThrottle)
                    {
                        Chat.Instance.SendMessage("/automove off");
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool? InteractWithVoyagePanel()
        {
            if (VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Svc.Targets.Target?.Address == obj.Address)
            {
                if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Interact", 1000))
                {
                    TargetSystem.Instance()->InteractWithObject(obj.Struct(), false);
                    return true;
                }
            }
            return false;
        }

        internal static bool? SelectAirshipManagement()
        {
            if (Utils.TrySelectSpecificEntry("Airship Management", () => EzThrottler.Throttle("Voyage.SelectManagement", 1000)))
            {
                return true;
            }
            return false;
        }

        internal static bool? SelectSubManagement()
        {
            if (Utils.TrySelectSpecificEntry("Submersible Management", () => EzThrottler.Throttle("Voyage.SelectManagement", 1000)))
            {
                return true;
            }
            return false;
        }

        internal static bool? SelectSubjectByName(string name)
        {
            if (Utils.TrySelectSpecificEntry((x) => x.StartsWith($"{name}."), () => EzThrottler.Throttle("Voyage.SelectSubjectByName", 1000)))
            {
                return true;
            }
            return false;
        }

        internal static bool? Redeploy()
        {
            if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
            {
                if(Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Redeploy"))
                {
                    Callback.Fire(addon, true, (int)1);
                    return true;
                }
            }
            return false;
        }

        internal static bool? Deploy()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Deploy"))
                {
                    Callback.Fire(addon, true, (int)0);
                    return true;
                }
            }
            return false;
        }
    }
}
