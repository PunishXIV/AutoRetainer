using AutoRetainer.Modules.Voyage.Tasks;
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
    internal unsafe static class VoyageScheduler
    {
        internal static void Log(string t) => VoyageUtils.Log(t);
        internal static bool Enabled = false;
        internal static bool? SelectQuitVesselMenu() => Utils.TrySelectSpecificEntry("Quit");

        internal static bool? ConfirmRepair()
        {
            if (TaskRepairAll.Abort) return true;
            var x = Utils.GetSpecificYesno((s) => s.ContainsAny(StringComparison.OrdinalIgnoreCase, "repair"));
            if(x != null && Utils.GenericThrottle)
            {
                Log("Confirming repair");
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
                    Log("Closing repair window (CompanyCraftSupply)");
                    Callback.Fire(addon, true, 5);
                    return true;
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("AirShipPartsMenu", out var addon2) && IsAddonReady(addon2))
            {
                if (Utils.GenericThrottle)
                {
                    Log("Closing repair window (AirShipPartsMenu)");
                    Callback.Fire(addon2, true, 5);
                    return true;
                }
            }
            return false;
        }

        internal static bool? TryRepair(int slot)
        {
            if (TaskRepairAll.Abort) return true;
            var t = $"VoyageScheduler.TryRepair{slot}";
            if (TryGetAddonByName<AtkUnitBase>("SelectYesno", out var _))
            {
                Log("Found yesno, repair request success");
                return true;
            }
            if (EzThrottler.Check(t))
            {
                if (TryGetAddonByName<AtkUnitBase>("CompanyCraftSupply", out var addon) && IsAddonReady(addon))
                {
                    if (Utils.GenericThrottle)
                    {
                        Callback.Fire(addon, true, (int)3, Utils.ZeroAtkValue, (int)slot, Utils.ZeroAtkValue, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                        EzThrottler.Throttle(t, 1000, true);
                        Log($"Executing CompanyCraftSupply repair request on slot {slot} ");
                        return false;
                    }
                }
                else if (TryGetAddonByName<AtkUnitBase>("AirShipPartsMenu", out var addon2) && IsAddonReady(addon2))
                {
                    if (Utils.GenericThrottle)
                    {
                        Callback.Fire(addon2, true, (int)3, Utils.ZeroAtkValue, (int)slot, Utils.ZeroAtkValue, Utils.ZeroAtkValue, Utils.ZeroAtkValue);
                        EzThrottler.Throttle(t, 1000, true);
                        Log($"Executing AirShipPartsMenu repair request on slot {slot} ");
                        return false;
                    }
                }
                else
                {
                    Utils.RethrottleGeneric();
                }
            }
            return false;
        }

        internal static bool? WaitForYesNoDisappear()
        {
            return !TryGetAddonByName<AtkUnitBase>("SelectYesno", out _);
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
                Log("Selecting cutscene skipping");
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
                        Log("Targeting workshop CP");
                        Svc.Targets.Target = obj;
                    }
                }
                else
                {
                    if (Utils.GenericThrottle)
                    {
                        Log("Locking on workshop CP");
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
                    Utils.RegenerateRandom();
                    return true;
                }
            }
            return false;
        }

        internal static bool? AutomoveOffPanel()
        {
            if (VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Svc.Targets.Target?.Address == obj.Address)
            {
                if (Vector3.Distance(obj.Position, Player.Object.Position) < 4f + Utils.Random*0.25f)
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
            if (VoyageUtils.TryGetNearestVoyagePanel(out var obj))
            {
                if (Svc.Targets.Target?.Address == obj.Address)
                {
                    if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Interact", 2000))
                    {
                        Log("Interacting with workshop CP");
                        TargetSystem.Instance()->InteractWithObject(obj.Struct(), false);
                        return true;
                    }
                }
                else
                {
                    if (obj.IsTargetable && Utils.GenericThrottle)
                    {
                        Svc.Targets.Target = obj;
                    }
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

        internal static bool? SelectExitMainPanel()
        {
            return Utils.TrySelectSpecificEntry(Lang.CancelVoyage, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.ExitMainPanel", 1000));
        }

        internal static bool? SelectVesselByName(string name)
        {
            return Utils.TrySelectSpecificEntry((x) => x.StartsWith($"{name}."), () => EzThrottler.Throttle("Voyage.SelectVesselByName", 1000));
        }

        internal static bool? SelectViewPreviousLog()
        {
            return Utils.TrySelectSpecificEntry(Lang.ViewPrevVoyageLog, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectViewPreviousLog", 1000));
        }

        internal static bool? RedeployVessel()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
            {
                var button = addon->UldManager.NodeList[3]->GetAsAtkComponentButton();
                if (!button->IsEnabled)
                {
                    EzThrottler.Throttle("Voyage.Redeploy", 500, true);
                    return false;
                }
                else
                {
                    if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Redeploy"))
                    {
                        Callback.Fire(addon, true, 1);
                        return true;
                    }
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

        internal static bool? CancelDeployVessel()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon) && IsAddonReady(addon))
            {
                if (Utils.GenericThrottle && EzThrottler.Throttle("Voyage.CancelDeploy"))
                {
                    Callback.Fire(addon, true, -1);
                    return true;
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? SelectQuitVesselSelectorMenu()
        {
            return Utils.TrySelectSpecificEntry(Lang.NothingVoyage, () => EzThrottler.Throttle("Voyage.Quit", 1000));
        }

    }
}
