using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks;
public unsafe static class TaskActivateSealSweetener
{
    public static long LastAttemptAt = 0;
    public static void EnqueueThrottled()
    {
        if(Player.Status.Any(x => x.StatusId == 414 || x.StatusId == 1078)) return;
        if(Environment.TickCount64 > LastAttemptAt + 12 * 60 * 1000)
        {
            LastAttemptAt = Environment.TickCount64;
            Enqueue();
        }
    }

    public static void Enqueue()
    {
        var oldNum = NumActions;
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon) && addon->IsReady())
            {
                return true;
            }
            else
            {
                if(EzThrottler.Throttle("OpenFcCmd"))
                {
                    Chat.ExecuteCommand("/freecompanycmd");
                }
                return false;
            }
        });
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonByName<AtkUnitBase>("FreeCompanyAction", out var addon) && addon->IsReady())
            {
                return true;
            }
            else
            {
                if(TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon2) && addon2->IsReady())
                {
                    if(EzThrottler.Throttle("OpenFcAction"))
                    {
                        Callback.Fire(addon2, true, 0, 4u);
                    }
                }
                return false;
            }
        });
        P.TaskManager.Enqueue(() => NumActions != oldNum, new(timeLimitMS:3000, abortOnTimeout:false));
        P.TaskManager.Enqueue(() =>
        {
            if(NumActions == 0)
            {
                CloseFCAddon();
                return true;
            }
            else
            {
                if(TryGetAddonByName<AtkUnitBase>("FreeCompanyAction", out var addon) && addon->IsReady())
                {
                    foreach(var action in (int[])[36, 6])
                    {
                        for(var i = 0; i < Actions.Count; i++)
                        {
                            var x = Actions[i];
                            if(x == action)
                            {
                                Callback.Fire(addon, true, 1, (uint)i);
                                P.TaskManager.InsertTask(new TaskManagerTask(() => 
                                {
                                    if(TryGetAddonMaster<AddonMaster.ContextMenu>(out var m) && m.IsAddonReady)
                                    {
                                        foreach(var entry in m.Entries)
                                        {
                                            if(entry.Text == Svc.Data.GetExcelSheet<Addon>().GetRow(2817).Text)
                                            {
                                                if(EzThrottler.Throttle("SelectMenuEntry"))
                                                {
                                                    entry.Select();
                                                    P.TaskManager.InsertTask(new(() =>
                                                    {
                                                        var yesno = Utils.GetSpecificYesno(Svc.Data.GetExcelSheet<CompanyAction>().GetRow((uint)x).Name.GetText());
                                                        if(yesno != null)
                                                        {
                                                            if(EzThrottler.Throttle("YesNoCompanyAction"))
                                                            {
                                                                new AddonMaster.SelectYesno(yesno).Yes();
                                                                P.TaskManager.Insert(CloseFCAddon);
                                                                return true;
                                                            }
                                                        }
                                                        return false;
                                                    }));
                                                    return true;
                                                }
                                            }
                                            if(entry.Text == Svc.Data.GetExcelSheet<Addon>().GetRow(632).Text)
                                            {
                                                CloseFCAddon();
                                                return true;
                                            }
                                        }
                                    }
                                    return false;
                                }));
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        });
    }

    static void CloseFCAddon()
    {
        if(TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon) && addon->IsReady())
        {
            Callback.Fire(addon, true, -1);
        }
    }

    public static int* Array => AtkStage.Instance()->GetNumberArrayData(NumberArrayType.FreeCompanyAction)->IntArray;
    public static int NumActions => Array[11];
    public static List<int> Actions
    {
        get
        {
            var ret = new List<int>();
            for(int i = 0; i < NumActions; i++)
            {
                ret.Add(Array[12 + 3 * i]);
            }
            return ret;
        }
    }
}