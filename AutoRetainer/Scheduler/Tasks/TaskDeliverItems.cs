using ECommons.GameHelpers;
using ECommons.Singletons;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
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