using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Singletons;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks;
public static unsafe class TaskDeliverItems
{
    public static bool Enqueue(bool force = false)
    {
        var gcInfo = GCContinuation.GetFullGCInfo();
        if(gcInfo == null)
        {
            Notify.Error("Not employed by a Grand Company");
            return false;
        }
        if(Data.GCDeliveryType == AutoRetainerAPI.Configuration.GCDeliveryType.Disabled)
        {
            Notify.Error("Can not enqueue GC delivery as it is disabled for current character");
            return false;
        }
        if(S.LifestreamIPC.IsBusy())
        {
            Notify.Error("Lifestream is busy");
            return false;
        }
        if(!force && Utils.IsBusy)
        {
            Notify.Error("AutoRetainer is busy");
            return false;
        }
        P.TaskManager.Enqueue(() =>
        {
            if(C.FullAutoGCDeliveryUseBuffItem)
            {
                if(Player.Object.IsCasting(14946, ActionType.Item))
                {
                    return true;
                }
                else if(!Player.Status.Any(x => x.StatusId == 1078) && InventoryManager.Instance()->GetInventoryItemCount(14946) > 0)
                {
                    if(EzThrottler.Throttle("UseFCBuffItem", 1000))
                    {
                        AgentInventoryContext.Instance()->UseItem(14946);
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }, new(timeLimitMS: 30000, abortOnTimeout: false));
        P.TaskManager.Enqueue(() => !Player.Object.IsCasting() && !Player.IsAnimationLocked);
        if(!gcInfo.IsReadyToExchange())
        {
            P.TaskManager.Enqueue(() => S.LifestreamIPC.ExecuteCommand("gc " + Player.GrandCompany switch
            {
                ECommons.ExcelServices.GrandCompany.ImmortalFlames => "if",
                ECommons.ExcelServices.GrandCompany.Maelstrom => "m",
                ECommons.ExcelServices.GrandCompany.TwinAdder => "ta",
                _ => throw new ArgumentOutOfRangeException()
            }));
        }
        P.TaskManager.Enqueue(() => !S.LifestreamIPC.IsBusy(), new(timeLimitMS: 5 * 60 * 1000) { CompanionAction = _ => EzThrottler.Throttle("GcBusy", 60000, true)});
        P.TaskManager.Enqueue(() => GCContinuation.EnqueueInitiation(true));
        return true;
    }
}