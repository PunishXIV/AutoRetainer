using ECommons.Automation;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Multi
{
    internal unsafe static class HouseEnterTask
    {
        internal static void EnqueueTask()
        {
            P.TaskManager.Enqueue(WaitUntilNotBusy);
            P.TaskManager.Enqueue(Turn);
            P.TaskManager.Enqueue(Approach);
            P.TaskManager.Enqueue(AutorunOff);
            P.TaskManager.Enqueue(() => { Chat.Instance.SendMessage("/automove off"); return true; });
            P.TaskManager.Enqueue(SetTarget);
            P.TaskManager.Enqueue(Interact);
            P.TaskManager.Enqueue(WaitUntilLeavingZone);
        }

        internal static bool WaitUntilNotBusy()
        {
            if (MultiMode.IsInteractionAllowed())
            {
                return true;
            }
            return false;
        }

        internal static bool Turn()
        {
            var entrance = Utils.GetNearestEntrance(out var d);
            if(entrance != null && d < 20f)
            {
                if(Utils.GetAngleTo(entrance.Position.ToVector2()).InRange(178, 183))
                {
                    return true;
                }
                else
                {
                    if(EzThrottler.Throttle("HET.Turn", 1000))
                    {
                        PluginLog.Debug($"Turning...");
                        P.Memory.Turn(entrance.Position);
                    }
                }
            }
            return false;
        }

        internal static bool Approach()
        {
            PluginLog.Debug($"Enabling");
            Chat.Instance.SendMessage("/automove on");
            return true;
        }
        
        internal static bool AutorunOff()
        {
            var entrance = Utils.GetNearestEntrance(out var d);
            if(entrance != null && d < 2f)
            {
                PluginLog.Debug($"Disabling automove");
                Chat.Instance.SendMessage("/automove off");
                return true;
            }
            return false;
        }

        internal static bool SetTarget()
        {
            var entrance = Utils.GetNearestEntrance(out var d);
            if (entrance != null && d < 5f)
            {
                PluginLog.Debug($"Setting entrance target");
                Svc.Targets.SetTarget(entrance);
                return true;
            }
            return false;
        }

        internal static bool Interact()
        {
            var entrance = Utils.GetNearestEntrance(out var d);
            if (entrance != null && Svc.Targets.Target?.Address == entrance.Address && EzThrottler.Throttle("HET.Interact", 1000))
            {
                PluginLog.Debug($"Interacting with entrance");
                TargetSystem.Instance()->InteractWithObject((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)entrance.Address);
                return true;
            }
            return false;
        }

        internal static bool WaitUntilLeavingZone()
        {
            return !ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType);
        }
    }
}
