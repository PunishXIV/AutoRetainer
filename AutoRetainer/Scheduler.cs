using AutoRetainer.Multi;
using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static AutoRetainer.Utils;

namespace AutoRetainer;

internal unsafe class Scheduler
{
    internal static Dictionary<string, long> Bans = new();
    internal static bool turbo = false;
    const int RetListLife = 200;
    internal static int RandomAddition = 0;
    static bool ChangeRandomAddition = false;
    static bool IsDoneConsolidating = false;

    internal static void Tick()
    {
        if (P.IsEnabled())
        {
            if (Svc.ClientState.LocalPlayer != null)
            {
                //nextTick = Environment.TickCount64 + 100;
                Safe(TickInternal);
            }
            AddonLifeTracker.Tick();
            
        }
        else
        {
            Clicker.lastAction = ActionType.None;
        }
    }

    static bool EnsureInventorySpace()
    {
        if (!Utils.IsInventoryFree())
        {
            Log("Inventory is full");
            DuoLog.Warning("Your inventory is full.");
            P.DisablePlugin();
            return false;
        }
        return true;
    }

    static void TickInternal()
    {
        if(Svc.ClientState.LocalPlayer?.HomeWorld.Id != Svc.ClientState.LocalPlayer?.CurrentWorld.Id)
        {
            Notify.Error("You are visiting different world");
            P.DisablePlugin();
            return;
        }
        if(ConfigModule.Instance()->GetIntValue((short)ConfigOption.IdlingCameraAFK) != 0)
        {
            DuoLog.Error("Please go to System settings - Other and disable idle afk camera.");
            P.DisablePlugin();
            return;
        }
        if (P.TaskManager.IsBusy)
        {
            return;
        }
        if (P.retainerManager.Ready) {
            var allRetainersBusy = true;
            for (var i = 0; i < P.retainerManager.Count; i++)
            {
                var r = P.retainerManager.Retainer(i);
                var t = r.GetVentureSecondsRemaining();
                if (t > 0)
                {
                    if (GetRemainingBanTime(r.Name.ToString()) <= t + -P.config.UnsyncCompensation + 1)
                    {
                        Ban(r.Name.ToString(), (int)t + -P.config.UnsyncCompensation + 1);
                    }
                }

                if(P.GetSelectedRetainers(Svc.ClientState.LocalContentId).Contains(r.Name.ToString()) && t < 10 * 60)
                {
                    allRetainersBusy = false;
                }
            }
            if(ChangeRandomAddition && allRetainersBusy)
            {
                ChangeRandomAddition = false;
                RandomAddition = new Random().Next(0, 5 * 60);
                Log("Regenerating random: " + RandomAddition);
            }
        }
        else
        {
            return;
        }
        if ((P.config.AutoUseRetainerBell && !MultiMode.Enabled) && GenericHelpers.IsNoConditions())
        {
            if (!P.config.AutoUseRetainerBellFocusOnly || MultiMode.Enabled || Svc.Targets.FocusTarget?.Name.ToString().EqualsIgnoreCaseAny(Consts.BellName, "リテイナーベル") == true)
            {
                if (GetNextRetainerName() != null && Clicker.IsClickAllowed())
                {
                    if (!EnsureInventorySpace()) return;
                    Clicker.InteractWithNearestBell(out _);
                }
            }
        }
        if (!Svc.Targets.Target.IsRetainerBell())
        {
            //PluginLog.Information($"{Svc.Targets.Target?.Name}");
            return;
        }
        if (Svc.Condition[ConditionFlag.OccupiedSummoningBell] && Svc.Targets.Target?.Name.ToString().EqualsIgnoreCaseAny(Consts.BellName, "リテイナーベル") == true && Clicker.IsClickAllowed())
        {
            if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && addon->IsVisible 
                && AddonLifeTracker.GetAge("RetainerList") > RetListLife && IsAddonReady(addon))
            {
                var retainer = GetNextRetainerName();
                if(retainer != null && IsInventoryFree())
                {
                    Log($"Retainer {retainer}");
                    Ban(retainer, 10 * 10);
                    Clicker.SelectRetainerByName(retainer);
                    IsDoneConsolidating = false;
                }
                else
                {
                    Clicker.lastAction = ActionType.None;
                    turbo = false;
                    if(ShouldAutoClose() && Clicker.IsClickAllowed())
                    {
                        if (Clicker.ClickClose())
                        {
                            EnsureInventorySpace();
                        }
                    }
                }
            }
            else if(TryGetAddonByName<AddonSelectString>("SelectString", out var select) && select->AtkUnitBase.IsVisible 
                && IsAddonReady(&select->AtkUnitBase) && IsCurrentRetainerEnabled())
            {
                var textNode = ((AtkTextNode*)select->AtkUnitBase.UldManager.NodeList[3]);
                var text = textNode->NodeText.ToString();
                //ImGui.SetClipboardText(text);
                if(Utils.TryParseRetainerName(text, out var retName) && Utils.TryGetRetainerByName(retName, out var retainer) && retainer.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation
                    && (!P.config.DontReassign || retainer.VentureID != 0))
                {
                    if (EnsureInventorySpace())
                    {
                        Log($"Retainer {retName} sending to venture");
                        Ban(retName, 10 * 10 + 10 + RandomAddition);
                        ChangeRandomAddition = true;
                        Clicker.SelectVentureMenu();
                    }
                    else
                    {
                        Clicker.SelectQuit();
                    }
                }
                else if(P.config.EnableAssigningQuickExploration && text.Equals(Consts.RetainerAskCategoryText))
                {
                    Log($"Selecting quick exploration");
                    Clicker.SelectQuickVenture();
                }
                else
                {
                    //if (true || IsDoneConsolidating)
                    {
                        Log($"Retainer {retName} exiting");
                        Clicker.SelectQuit();
                    }
                    /*else
                    {
                        if (P.config.SS)
                        {
                            DuoLog.Information($"Instead of exiting, injecting additional tasks");
                            var x = () =>
                            {
                                if (new Random().Next(0, 50) == 0)
                                {
                                    DuoLog.Information($"Task simulation completed!");
                                    return true;
                                }
                                return false;
                            };

                            P.TaskManager.Enqueue(x);
                            P.TaskManager.Enqueue(x);
                            P.TaskManager.Enqueue(x);
                        }
                        IsDoneConsolidating = true;
                    }*/
                }
            }
            else if (TryGetAddonByName<AddonRetainerTaskAsk>("RetainerTaskAsk", out var addon3) && IsAddonReady(&addon3->AtkUnitBase) && IsCurrentRetainerEnabled())
            {
                Log($"Sending retainer to venture");
                Clicker.ClickRetainerTaskAsk();
            }
            else if (TryGetAddonByName<AddonRetainerTaskResult>("RetainerTaskResult", out var addon4) && IsAddonReady(&addon4->AtkUnitBase) && IsCurrentRetainerEnabled())
            {
                Log($"Confirming result");
                Clicker.ClickReassign(!P.config.DontReassign);
            }
        }
    }

    internal static bool ShouldAutoClose()
    {

        return (!MultiMode.Enabled && P.config.AutoUseRetainerBell && P.config.AutoCloseRetainerWindow) || (MultiMode.Enabled && (!MultiMode.IsAnySelectedRetainerFinishesWithin(60) || !MultiMode.EnsureCharacterValidity())) ;

    }

    static void Log(String s)
    {
        InternalLog.Debug("[Scheduler] " + s);
    }

    internal static bool IsBanned(string name)
    {
        if(Bans.TryGetValue($"{Svc.ClientState.LocalContentId}@{name}", out var x) && Environment.TickCount64 < x)
        {
            return true;
        }
        return false;
    }

    internal static long GetRemainingBanTime(string name)
    {
        if (Bans.TryGetValue($"{Svc.ClientState.LocalContentId}@{name}", out var x) && Environment.TickCount64 < x)
        {
            return Math.Max((x - Environment.TickCount64) / 1000, 0);
        }
        return 0;
    }

    internal static void Ban(string name, int seconds = 10 * 10 + 10)
    {
        Bans[$"{Svc.ClientState.LocalContentId}@{name}"] = Environment.TickCount64 + seconds * 1000;
        Log($"Banned interactions with retainer {name} for {seconds}");
    }

    static internal string GetNextRetainerName()
    {
        if (P.retainerManager.Ready)
        {
            for (var i = 0; i < P.retainerManager.Count; i++)
            {
                var r = P.retainerManager.Retainer(i);
                var rname = r.Name.ToString();
                if (P.GetSelectedRetainers(Svc.ClientState.LocalContentId).Contains(rname)
                    && r.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation
                    && (r.VentureID != 0 || P.config.EnableAssigningQuickExploration)
                    && !IsBanned(rname))
                {
                    return rname;
                }
            }
        }
        return null;
    }
}
