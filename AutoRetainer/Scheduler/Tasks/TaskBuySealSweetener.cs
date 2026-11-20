using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.ExcelServices.Sheets;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;
using System.Collections.Generic;
using System.Text;
using TerraFX.Interop.Windows;

namespace AutoRetainer.Scheduler.Tasks;

public unsafe static class TaskBuySealSweetener
{
    public static void Enqueue()
    {
        var conf = new TaskManagerConfiguration()
        {
            DiscardGuid = Guid.NewGuid(),
        };
        P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(() => Svc.Objects.FirstOrDefault(x => x.DataId.EqualsAny(ValidOfficerDataID)), configuration:conf));
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonMaster<AddonMaster.SelectString>(out var m) && m.IsAddonReady)
            {
                if(m.Entries.TryGetFirst(x => x.Text == Choice, out var entry) && EzThrottler.Throttle("SelectEntrySweetener"))
                {
                    entry.Select();
                    return false;
                }
            }
            if(TryGetAddonMaster<AddonMaster.FreeCompanyExchange>(out var fcm) && fcm.IsAddonReady && fcm.Count > 0) return true;
            return false;
        }, conf);
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonMaster<AddonMaster.FreeCompanyExchange>(out var fcm) && fcm.IsAddonReady)
            {
                {
                    if(IsSufficientRankForLv2() && fcm.Entries.TryGetFirst(x => x.Action.RowId == 36, out var entry) && Utils.FCPoints >= entry.SealsRequired && EzThrottler.Throttle("BuyAction"))
                    {
                        entry.Buy();
                        return true;
                    }
                }
                {
                    if(IsSufficientRank() && fcm.Entries.TryGetFirst(x => x.Action.RowId == 6, out var entry) && Utils.FCPoints >= entry.SealsRequired && EzThrottler.Throttle("BuyAction"))
                    {
                        entry.Buy();
                        return true;
                    }
                }
            }
            return false;
        }, conf);
        P.TaskManager.Enqueue(() =>
        {
            var yn = Utils.GetSpecificYesno([Lumina.Excel.Sheets.Addon.Get(3422).Text.GetText(true)]);
            if(yn != null)
            {
                return true;
            }
            return false;
        }, conf);
        P.TaskManager.Enqueue(() =>
        {
            var yn = Utils.GetSpecificYesno([Lumina.Excel.Sheets.Addon.Get(3422).Text.GetText(true)]);
            if(yn != null)
            {
                if(yn->IsReady() && EzThrottler.Throttle("SelectYes"))
                {
                    new AddonMaster.SelectYesno(yn).Yes();
                    return false;
                }
            }
            else
            {
                return true;
            }
            return false;
        }, conf);
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonMaster<AddonMaster.FreeCompanyExchange>(out var fcm))
            {
                if(fcm.IsAddonReady)
                {
                    if(EzThrottler.Throttle("CloseFcm"))
                    {
                        Callback.Fire(fcm.Addon, true, -1);
                        return false;
                    }
                }
            }
            else
            {
                return true;
            }
            return false;
        });
    }

    public unsafe static bool IsSufficientRank()
    {
        return InfoProxyFreeCompany.Instance()->Rank >= 5;
    }

    public unsafe static bool IsSufficientRankForLv2()
    {
        return InfoProxyFreeCompany.Instance()->Rank >= 8;
    }

    static uint[] ValidOfficerDataID = [1003925, 1002389, 1000165];
    static string Choice = Svc.Data.GetExcelSheet<QuestDialogueText>(name: "custom/001/ComDefFreeCompanyReward_00100").GetRow(2).Value.GetText();

    
}
