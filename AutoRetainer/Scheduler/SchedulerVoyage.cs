using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Interop;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoRetainer.Scheduler
{
    internal unsafe static class SchedulerVoyage
    {
        internal static bool? WaitForCutscene()
        {
            return Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                || Svc.Condition[ConditionFlag.WatchingCutscene78];
        }

        internal static bool? PressEsc()
        {
            var nLoading = Svc.GameGui.GetAddonByName("NowLoading", 1);
            if (nLoading != IntPtr.Zero)
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
            string[] SkipCutsceneStr = { "Skip cutscene?", "要跳过这段过场动画吗？", "要跳過這段過場動畫嗎？", "Videosequenz überspringen?", "Passer la scène cinématique ?", "このカットシーンをスキップしますか？" };
            var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
            if (addon == IntPtr.Zero) return false;
            var selectStrAddon = (AddonSelectString*)addon;
            if (!IsAddonReady(&selectStrAddon->AtkUnitBase))
            {
                return false;
            }
            PluginLog.Debug($"1: {selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString()}");
            if (!SkipCutsceneStr.Contains(selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString())) return false;
            if(Utils.GenericThrottle && EzThrottler.Throttle("Voyage.CutsceneSkip"))
            {
                PluginLog.Debug("Selecting cutscene skipping");
                ClickSelectString.Using(addon).SelectItem(0);
                return true;
            }
            return false;
        }

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
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? SelectSubManagement()
        {
            if (Utils.TrySelectSpecificEntry("Submersible Management", () => EzThrottler.Throttle("Voyage.SelectManagement", 1000)))
            {
                return true;
            }
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? SelectSubjectByName(string name)
        {
            if (Utils.TrySelectSpecificEntry((x) => x.StartsWith($"{name}."), () => EzThrottler.Throttle("Voyage.SelectSubjectByName", 1000)))
            {
                return true;
            }
            else
            {
                Utils.RethrottleGeneric();
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
            else
            {
                Utils.RethrottleGeneric();
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
            else
            {
                Utils.RethrottleGeneric();
            }
            return false;
        }

        internal static bool? Quit()
        {
            if (Utils.TrySelectSpecificEntry("Nothing.", () => EzThrottler.Throttle("Voyage.Quit", 1000)))
            {
                return true;
            }
            return false;
        }

    }
}
