using AutoRetainer.Modules.Voyage;

using Dalamud.Game.ClientState.Conditions;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Modules;

internal static unsafe class MiniTA
{
    internal static void Tick()
    {
        if(!IPC.Suppressed)
        {
            if(VoyageScheduler.Enabled)
            {
                ConfirmCutsceneSkip();
                ConfirmRepair();
            }
            if(P.TaskManager.IsBusy || (Svc.Condition[ConditionFlag.OccupiedSummoningBell] && (SchedulerMain.PluginEnabled || P.TaskManager.IsBusy || P.ConditionWasEnabled)))
            {
                if(TryGetAddonByName<AddonTalk>("Talk", out var addon) && addon->AtkUnitBase.IsVisible)
                {
                    new AddonMaster.Talk((nint)addon).Click();
                }
            }
            if(C.SkipItemConfirmations && (P.TaskManager.IsBusy || AutoGCHandin.Operation))
            {
                SkipItemConfirmations();
            }
        }
    }

    internal static void SkipItemConfirmations()
    {
        //397	This item has materia attached. Are you certain you wish to sell it?
        //398	Your spiritbond with this item is 100%. Are you certain you wish to sell it?
        //399 This item is unique and untradable.Are you certain you wish to sell it?
        //4477  Are you certain you wish to sell this item ?
        //102433	Do you really want to trade an item with materia affixed? The materia will be lost.
        //102434	Do you really want to trade a high-quality item?
        var x = Utils.GetSpecificYesno(s => s.Cleanup().ContainsAny(StringComparison.OrdinalIgnoreCase, Ref<string[]>.Get("Skip", () => ((uint[])[397, 398, 399, 4477, 102433, 102434]).Select(a => Svc.Data.GetExcelSheet<Addon>().GetRow(a).Text.ExtractText().Cleanup()).ToArray())));
        if(x != null && IsAddonReady(x))
        {
            new AddonMaster.SelectYesno(x).Yes();
        }
    }

    internal static void ConfirmRepair()
    {
        var x = Utils.GetSpecificYesno((s) => s.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.WorkshopRepairConfirm));
        if(x != null && Utils.GenericThrottle)
        {
            VoyageUtils.Log("Confirming repair");
            new AddonMaster.SelectYesno((nint)x).Yes();
        }
    }

    internal static void ConfirmCutsceneSkip()
    {
        var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
        if(addon == IntPtr.Zero) return;
        var selectStrAddon = (AddonSelectString*)addon;
        if(!IsAddonReady(&selectStrAddon->AtkUnitBase))
        {
            return;
        }
        //PluginLog.Debug($"1: {selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString()}");
        if(!Lang.SkipCutsceneStr.Contains(selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString())) return;
        if(EzThrottler.Throttle("SkipCutsceneConfirm"))
        {
            PluginLog.Debug("Selecting cutscene skipping");
            new AddonMaster.SelectString(addon).Entries[0].Select();
        }
    }

    internal static bool ProcessCutsceneSkip(nint arg)
    {
        return VoyageScheduler.Enabled;
    }
}
