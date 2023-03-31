using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Scheduler;

internal unsafe static class SchedulerMain
{
    internal static bool PluginEnabledInternal;
    internal static bool PluginEnabled 
    { 
        get 
        {
            return PluginEnabledInternal && !IPC.Suppressed;
        }
        private set 
        {
            PluginEnabledInternal = value;
        } 
    }

    internal static PluginEnableReason Reason { get; private set; }

    internal static bool? EnablePlugin(PluginEnableReason reason)
    {
        Reason = reason;
        PluginEnabled = true;
        P.DebugLog($"Plugin is enabled, reason: {reason}");
        return true;
    }

    internal static bool? DisablePlugin() 
    {
        PluginEnabled = false;
        P.DebugLog($"Plugin disabled");
        return true;
    }

    internal static void Tick()
    {
        if (PluginEnabled)
        {
            if (TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && addon->IsVisible)
            {
                if (Utils.GenericThrottle)
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


                                    var adata = Utils.GetAdditionalData(Svc.ClientState.LocalContentId, ret.Name.ToString());

                                    if (adata.VenturePlanner.Count == 0)
                                    {
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
                                    }
                                    else
                                    {
                                        var next = adata.GetNextPlannedVenture();
                                        if (ret.VentureID != 0)
                                        {
                                            if (next == ret.VentureID)
                                            {
                                                TaskReassignVenture.Enqueue();
                                            }
                                            else
                                            {
                                                TaskCollectVenture.Enqueue();
                                                if(VentureUtils.GetVentureById(next).IsFieldExploration())
                                                {
                                                    TaskAssignFieldExploration.Enqueue(next);
                                                }
                                                else
                                                {
                                                    TaskAssignHuntingVenture.Enqueue(next);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (VentureUtils.GetVentureById(next).IsFieldExploration())
                                            {
                                                TaskAssignFieldExploration.Enqueue(next);
                                            }
                                            else
                                            {
                                                TaskAssignHuntingVenture.Enqueue(next);
                                            }
                                        }
                                        adata.LastVenture = next;
                                    }

                                    //entrust duplicates
                                    if (adata.EntrustDuplicates)
                                    {
                                        TaskEntrustDuplicates.Enqueue();
                                    }

                                    //withdraw gil
                                    if (adata.WithdrawGil)
                                    {
                                        if (adata.Deposit)
                                        {
                                            if (TaskDepositGil.Gil > 0) TaskDepositGil.Enqueue(adata.WithdrawGilPercent);
                                        }
                                        else
                                        {
                                            TaskWithdrawGil.Enqueue(adata.WithdrawGilPercent);
                                        }
                                    }

                                    if (P.config.RetainerMenuDelay > 0)
                                    {
                                        TaskWaitSelectString.Enqueue(P.config.RetainerMenuDelay);
                                    }
                                    P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                                }
                            }
                            else
                            {
                                if ((P.config.Stay5 || MultiMode.Active) && !Utils.IsAllCurrentCharacterRetainersHaveMoreThan5Mins())
                                {
                                    //nothing
                                }
                                else
                                {
                                    if (Reason == PluginEnableReason.MultiMode)
                                    {
                                        P.DebugLog($"Scheduling closing and disabling plugin as MultiMode is running");
                                        P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                        P.TaskManager.Enqueue(DisablePlugin);
                                    }
                                    else
                                    {
                                        void Process(TaskCompletedBehavior behavior)
                                        {
                                            P.DebugLog($"Behavior: {behavior}");
                                            if (behavior.EqualsAny(TaskCompletedBehavior.Stay_in_retainer_list_and_disable_plugin, TaskCompletedBehavior.Close_retainer_list_and_disable_plugin))
                                            {
                                                P.DebugLog($"Scheduling plugin disabling (behavior={behavior})");
                                                P.TaskManager.Enqueue(DisablePlugin);
                                            }
                                            if (behavior.EqualsAny(TaskCompletedBehavior.Close_retainer_list_and_disable_plugin, TaskCompletedBehavior.Close_retainer_list_and_keep_plugin_enabled))
                                            {
                                                P.DebugLog($"Scheduling retainer list closing (behavior={behavior})");
                                                P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                            }
                                        }

                                        if (Reason == PluginEnableReason.Auto)
                                        {
                                            Process(P.config.TaskCompletedBehaviorAuto);
                                        }
                                        else if (Reason == PluginEnableReason.Manual)
                                        {
                                            Process(P.config.TaskCompletedBehaviorManual);
                                        }
                                        else if (Reason == PluginEnableReason.Access)
                                        {
                                            Process(P.config.TaskCompletedBehaviorAccess);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (EzThrottler.Throttle("CloseRetainerList", 1000))
                            {
                                DuoLog.Warning($"Your inventory is full");
                                if (MultiMode.Active)
                                {
                                    P.DebugLog($"Scheduling retainer list closing (multi mode)");
                                    P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                }
                                else
                                {
                                    void Process(TaskCompletedBehavior behavior)
                                    {
                                        P.DebugLog($"Behavior: {behavior}");
                                        if (behavior.EqualsAny(TaskCompletedBehavior.Close_retainer_list_and_disable_plugin, TaskCompletedBehavior.Close_retainer_list_and_keep_plugin_enabled))
                                        {
                                            P.DebugLog($"Scheduling retainer list closing (behavior={behavior})");
                                            P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                        }
                                    }

                                    if (Reason == PluginEnableReason.Auto)
                                    {
                                        Process(P.config.TaskCompletedBehaviorAuto);
                                    }
                                    else if (Reason == PluginEnableReason.Manual)
                                    {
                                        Process(P.config.TaskCompletedBehaviorManual);
                                    }
                                    else if (Reason == PluginEnableReason.Access)
                                    {
                                        Process(P.config.TaskCompletedBehaviorAccess);
                                    }
                                }
                                DisablePlugin();
                            }
                        }
                    }
                }
            }
            else
            {
                //DuoLog.Information($"123");
                if(P.config.OldRetainerSense)
                {
                    if (!IsOccupied() && Utils.AnyRetainersAvailableCurrentChara())
                    {
                        if (EzThrottler.Throttle("InteractWithBellGeneralEnqueue", 5000))
                        {
                            TaskInteractWithNearestBell.Enqueue();
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
                    && r.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation && (r.VentureID != 0 || P.config.EnableAssigningQuickExploration || Utils.GetAdditionalData(Svc.ClientState.LocalContentId, rname).VenturePlanner.Count > 0))
                {
                    return rname;
                }
            }
        }
        return null;
    }
}
