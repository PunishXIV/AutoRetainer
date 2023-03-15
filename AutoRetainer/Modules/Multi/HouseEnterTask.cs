using AutoRetainer.Helpers;
using ClickLib.Clicks;
using ECommons.Automation;
using ECommons.Events;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Multi
{
    internal unsafe static class HouseEnterTask
    {
        internal static void EnqueueTask()
        {
            P.TaskManager.Enqueue(YesAlready.WaitForYesAlreadyDisabledTask);
            P.TaskManager.Enqueue(WaitUntilNotBusy, 180 * 1000);
            P.TaskManager.Enqueue(() => SetTarget(20f));
            P.TaskManager.Enqueue(Lockon);
            P.TaskManager.Enqueue(Approach);
            P.TaskManager.Enqueue(AutorunOff);
            P.TaskManager.Enqueue(() => { Chat.Instance.SendMessage("/automove off"); return true; });
            P.TaskManager.Enqueue(() => SetTarget(5f));
            P.TaskManager.Enqueue(Interact);
            P.TaskManager.Enqueue(SelectYesno);
            P.TaskManager.Enqueue(WaitUntilLeavingZone);
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
            var entrance = Utils.GetNearestEntrance(out var d);
            if (entrance != null && d < 20f && EzThrottler.Throttle("HET.Lockon"))
            {
                /*if(Utils.GetAngleTo(entrance.Position.ToVector2()).InRange(178, 183))
                {
                    return true;
                }
                else
                {
                    if(EzThrottler.Throttle("HET.Turn", 1000))
                    {
                        P.DebugLog($"Turning...");
                        P.Memory.Turn(entrance.Position);
                    }
                }*/
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
                Svc.Targets.SetTarget(entrance);
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

        internal static bool? SelectYesno()
        {
            if (!ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType))
            {
                return null;
            }
            var addon = Utils.GetSpecificYesno("Enter the estate hall?");
            if (addon != null && IsAddonReady(addon) && EzThrottler.Throttle("HET.SelectYesno"))
            {
                P.DebugLog("Select yes");
                ClickSelectYesNo.Using((nint)addon).Yes();
                return true;
            }
            return false;
        }

        internal static bool? WaitUntilLeavingZone()
        {
            return !ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType);
        }
    }
}
