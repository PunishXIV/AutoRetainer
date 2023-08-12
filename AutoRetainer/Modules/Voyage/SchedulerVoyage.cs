using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Windows.Forms;

namespace AutoRetainer.Modules.Voyage
{
    internal unsafe static class SchedulerVoyage
    {
        internal static bool? SelectVesselQuit() => Utils.TrySelectSpecificEntry("Quit");

        internal static bool? ConfirmRepair()
        {
            var x = Utils.GetSpecificYesno((s) => s.ContainsAny(StringComparison.OrdinalIgnoreCase, "magitekrepairmaterials"));
            if(x != null && Utils.GenericThrottle)
            {
                ClickSelectYesNo.Using((nint)x).Yes();
                return true;
            }
            return false;
        }

        internal static bool? CloseRepair()
        {
            if (TryGetAddonByName<AtkUnitBase>("CompanyCraftSupply", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle)
                {
                    Callback.Fire(addon, true, 5);
                    return true;
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("AirShipPartsMenu", out var addon2) && IsAddonReady(addon2))
            {
                if (Utils.GenericThrottle)
                {
                    Callback.Fire(addon2, true, 5);
                    return true;
                }
            }
            return false;
        }

        internal static bool? TryRepair(int slot)
        {
            if (TryGetAddonByName<AtkUnitBase>("CompanyCraftSupply", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle)
                {
                    Callback.Fire(addon, true, (int)3, Utils.ZeroAtkValue, (int)slot, Utils.ZeroAtkValue, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                    return true;
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("AirShipPartsMenu", out var addon2) && IsAddonReady(addon2))
            {
                if (Utils.GenericThrottle)
                {
                    Callback.Fire(addon2, true, (int)3, Utils.ZeroAtkValue, (int)slot, Utils.ZeroAtkValue, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? WaitForCutscene()
        {
            return Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                || Svc.Condition[ConditionFlag.WatchingCutscene78];
        }

        internal static bool? PressEsc()
        {
            var nLoading = Svc.GameGui.GetAddonByName("NowLoading", 1);
            if (nLoading != nint.Zero)
            {
                var nowLoading = (AtkUnitBase*)nLoading;
                if (nowLoading->IsVisible)
                {
                    //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Now loading visible");
                }
                else
                {
                    //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Now loading not visible");
                    if (WindowsKeypress.SendKeypress(Keys.Escape))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool? ConfirmSkip()
        {
            
            var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
            if (addon == nint.Zero) return false;
            var selectStrAddon = (AddonSelectString*)addon;
            if (!IsAddonReady(&selectStrAddon->AtkUnitBase))
            {
                return false;
            }
            //PluginLog.Debug($"1: {selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString()}");
            if (!Lang.SkipCutsceneStr.Contains(selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString())) return false;
            if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.CutsceneSkip"))
            {
                PluginLog.Debug("Selecting cutscene skipping");
                ClickSelectString.Using(addon).SelectItem(0);
                return true;
            }
            return false;
        }

        internal static bool? Lockon()
        {
            if (VoyageUtils.TryGetNearestVoyagePanel(out var obj))
            {
                if (Svc.Targets.Target?.Address != obj.Address)
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
                if (Vector2.Distance(obj.Position.ToVector2(), Player.Object.Position.ToVector2()) < 2f)
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
            return Utils.TrySelectSpecificEntry(Lang.AirshipManagement, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectManagement", 1000));
        }

        internal static bool? SelectSubManagement()
        {
            return Utils.TrySelectSpecificEntry(Lang.SubmarineManagement, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectManagement", 1000));
        }

        internal static bool? SelectVesselByName(string name)
        {
            return Utils.TrySelectSpecificEntry((x) => x.StartsWith($"{name}."), () => EzThrottler.Throttle("Voyage.SelectVesselByName", 1000));
        }

        internal static bool? SelectViewPreviousLog()
        {
            return Utils.TrySelectSpecificEntry("View previous voyage log", () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectViewPreviousLog", 1000));
        }

        internal static bool? RedeployVessel()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Redeploy"))
                {
                    Callback.Fire(addon, true, 1);
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? FinalizeVessel()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Redeploy"))
                {
                    Callback.Fire(addon, true, 0);
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? DeployVessel()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Deploy"))
                {
                    Callback.Fire(addon, true, 0);
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? QuitVesselMenu()
        {
            return Utils.TrySelectSpecificEntry(Lang.NothingVoyage, () => EzThrottler.Throttle("Voyage.Quit", 1000));
        }

    }
}
