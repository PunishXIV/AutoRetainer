using Dalamud.Utility;
using ECommons.Automation;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Scheduler.Tasks;
public static unsafe class TaskChangeCharacter
{
    public static void Enqueue(string currentWorld, string charaName, string charaWorld, int account)
    {
        if(Svc.ClientState.IsLoggedIn)
        {
            EnqueueLogout();
        }
        EnqueueLogin(currentWorld, charaName, charaWorld, account);
    }

    public static void EnqueueLogout()
    {
        P.TaskManager.Enqueue(Logout);
        P.TaskManager.Enqueue(SelectYesLogout, new(timeLimitMS:100000));
    }

    public static void EnqueueLogin(string currentWorld, string charaName, string charaWorld, int account)
    {
        BailoutManager.IsLogOnTitleEnabled = false;
        var dc = (int)ExcelWorldHelper.Get(currentWorld).DataCenter.Row;
        P.TaskManager.Enqueue(ClickSelectDataCenter, new(timeLimitMS: 1000000));
        P.TaskManager.Enqueue(() => SelectDataCenter(dc), $"Connect to DC {dc}");
        P.TaskManager.Enqueue(() => SelectServiceAccount(account), $"SelectServiceAccount {account}");
        P.TaskManager.Enqueue(() => SelectCharacter(charaName, charaWorld), $"Select chara {charaName}@{charaWorld}", new(timeLimitMS: 1000000));
        P.TaskManager.Enqueue(ConfirmLogin);
    }

    public static bool? SelectYesLogout()
    {
        if(!Svc.ClientState.IsLoggedIn) return true;
        var addon = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115)?.Text.ToDalamudString().ExtractText());
        if(addon == null || !IsAddonReady(addon)) return false;
        if(Utils.GenericThrottle && EzThrottler.Throttle("ConfirmLogout"))
        {
            new AddonMaster.SelectYesno((nint)addon).Yes();
            return false;
        }
        return false;
    }

    public static bool? Logout()
    {
        if(C.DontLogout) return null;
        var addon = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115)?.Text.ToDalamudString().ExtractText());
        if(addon != null) return true;
        var isLoggedIn = Svc.Condition.Any();
        if(!isLoggedIn) return true;

        if(Player.Interactable && !Player.IsAnimationLocked && Utils.GenericThrottle && EzThrottler.Throttle("InitiateLogout"))
        {
            Chat.Instance.ExecuteCommand("/logout");
            return false;
        }
        return false;
    }

    public static bool? SelectServiceAccount(int account)
    {
        if(TryGetAddonByName<AtkUnitBase>("_CharaSelectWorldServer", out _))
        {
            return true;
        }
        if(TryGetAddonMaster<AddonMaster.SelectString>(out var m) && m.IsAddonReady)
        {
            var compareTo = Svc.Data.GetExcelSheet<Lobby>()?.GetRow(11)?.Text.ExtractText();
            if(m.Text == compareTo)
            {
                m.Entries[account].Select();
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? ClickSelectDataCenter()
    {
        if(TryGetAddonByName<AtkUnitBase>("TitleDCWorldMap", out var addon) && addon->IsVisible)
        {
            PluginLog.Information($"Visible");
            Utils.RethrottleGeneric();
            return true;
        }
        if(TryGetAddonMaster<AddonMaster._TitleMenu>(out var m) && m.IsReady)
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("ClickTitleMenuStart"))
            {
                m.DataCenter();
                return false;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? SelectDataCenter(int dc)
    {
        if(TryGetAddonMaster<AddonMaster.TitleDCWorldMap>(out var m) && m.IsAddonReady)
        {
            if(Utils.GenericThrottle && EzThrottler.Throttle("ClickDCSelect"))
            {
                m.Select(dc);
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static (string Name, string World)? Expected = null;

    public static bool? SelectCharacter(string name, string world)
    {
        Expected = (name, world);
        if(TryGetAddonByName<AtkUnitBase>("SelectYesno", out _))
        {
            Utils.RethrottleGeneric();
            return true;
        }
        if(TryGetAddonByName<AtkUnitBase>("SelectOk", out _))
        {
            Utils.RethrottleGeneric();
            return true;
        }
        if(TryGetAddonMaster<AddonMaster._CharaSelectListMenu>(out var m) && m.IsAddonReady && TryGetAddonMaster<AddonMaster._CharaSelectWorldServer>(out var mw))
        {
            if(m.TemporarilyLocked) return false;
            if(mw.Worlds.Length == 0) return false;
            foreach(var c in m.Characters)
            {
                if(c.Name == name && ExcelWorldHelper.GetName(c.HomeWorld) == world)
                {
                    if(Utils.GenericThrottle && EzThrottler.Throttle("SelectChara"))
                    {
                        /*if (!c.IsSelected)
                        {
                            c.Select();
                        }
                        else
                        {
                            c.Login();
                        }*/
                        c.Login();
                    }
                    return false;
                }
            }
            foreach(var w in mw.Worlds)
            {
                if(w.Name == world)
                {
                    if(Utils.GenericThrottle && EzThrottler.Throttle("SelectWorld"))
                    {
                        w.Select();
                    }
                    return false;
                }
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    public static bool? ConfirmLogin()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectOk", out _))
        {
            return true;
        }
        if(TryGetAddonMaster<AddonMaster.SelectYesno>(out var m) && m.IsAddonReady)
        {
            if(m.Text.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.LogInPartialText))
            {
                if(Utils.GenericThrottle && EzThrottler.Throttle("ConfirmLogin"))
                {
                    m.Yes();
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
}
