﻿using AutoRetainer.NewScheduler.Handlers;
using AutoRetainer.NewScheduler.Tasks;
using AutoRetainer.Offline;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.NewScheduler
{
    internal unsafe static class SchedulerMain
    {
        internal static bool Enabled = false;

        internal static void Tick()
        {
            if (Enabled)
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
                                        if (P.config.EnableAssigningQuickExploration)
                                        {
                                            TaskAssignQuickVenture.Enqueue();
                                        }
                                    }

                                    //entrust duplicates
                                    if (OfflineDataManager.GetData(ret.Name).EntrustDuplicates)
                                    {
                                        TaskEntrustDuplicates.Enqueue();
                                    }

                                    //withdraw gil
                                    if (OfflineDataManager.GetData(ret.Name).WithdrawGil)
                                    {
                                        TaskWithdrawGil.Enqueue(OfflineDataManager.GetData(ret.Name).WithdrawGilPercent);
                                    }

                                    P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                                }
                            }
                            else
                            {
                                P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                Enabled = false;
                            }
                        }
                        else
                        {
                            if (EzThrottler.Throttle("CloseRetainerList", 1000))
                            {
                                DuoLog.Warning($"Your inventory is full");
                                P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                Enabled = false;
                            }
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