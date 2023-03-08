using AutoRetainer.NewScheduler.Handlers;
using AutoRetainer.NewScheduler.Tasks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.NewScheduler
{
    internal unsafe static class NewScheduler
    {
        internal static bool Enabled = false;

        internal static void Tick()
        {
            if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && addon->IsVisible && Utils.GenericThrottle)
            {
                if (!P.TaskManager.IsBusy)
                {
                    if (Utils.IsInventoryFree())
                    {
                        var retainer = GetNextRetainerName();
                        if (retainer != null && Utils.TryGetRetainerByName(retainer, out var ret))
                        {
                            if (EzThrottler.Throttle("ScheduleSelectRetainer", 5000))
                            {
                                P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(retainer));

                                //resend retainer

                                if (P.config.EnabledTasks.Contains(TaskType.ManageVenture))
                                {
                                    if (ret.VentureID != 0)
                                    {
                                        if (P.config.DontReassign)
                                        {
                                            TaskCollectVenture.Enqueue();
                                        }
                                        else
                                        {
                                            TaskReassignVenture.Enqueue();
                                        }
                                    }
                                    else
                                    {
                                        if(P.config.EnableAssigningQuickExploration)
                                        {
                                            TaskAssignQuickVenture.Enqueue();
                                        }
                                    }
                                }

                                //entrust duplicates
                                if (P.config.EnabledTasks.Contains(TaskType.EntrustDuplicates))
                                {
                                    TaskEntrustDuplicates.Enqueue();
                                }

                                //withdraw gil
                                if (P.config.EnabledTasks.Contains(TaskType.WithdrawGil))
                                {
                                    TaskWithdrawGil.Enqueue(100);
                                }
                            }
                        }
                        else
                        {
                            RetainerListHandlers.CloseRetainerList();
                        }
                    }
                    else
                    {
                        if (EzThrottler.Throttle("CloseRetainerList", 1000))
                        {
                            DuoLog.Warning($"Your inventory is full");
                            RetainerListHandlers.CloseRetainerList();
                        }
                    }
                }
            }
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
                        && r.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation)
                    {
                        return rname;
                    }
                }
            }
            return null;
        }
    }
}
