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

    internal static volatile uint VentureOverride = 0;

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
                                if (EzThrottler.Throttle("ScheduleSelectRetainer", 2000))
                                {
                                    P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(retainer));

                                    var adata = Utils.GetAdditionalData(Svc.ClientState.LocalContentId, ret.Name.ToString());
                                    VentureOverride = 0;
                                    Svc.PluginInterface.GetIpcProvider<string, object>("AutoRetainer.OnSendRetainerToVenture").SendMessage(retainer);

                                    if(VentureOverride > 0)
                                    {
                                        P.DebugLog($"Using VentureOverride = {VentureOverride}");
                                        ret.ProcessVenturePlanner(VentureOverride);
                                    }
                                    else if (!adata.IsVenturePlannerActive())
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
                                        P.DebugLog($"Next planned venture: {next}");
                                        var completed = adata.IsLastPlannedVenture();
                                        P.DebugLog($"Is last planned venture: {completed}");
                                        if(next == 0)
                                        {
                                            var t = ($"Next venture ID is zero, planner is to be disabled");
                                            if(!completed)
                                            {
                                                DuoLog.Warning(t);
                                            }
                                            else
                                            {
                                                P.DebugLog(t);
                                            }
                                        }
                                        if (next == 0 || (completed && adata.VenturePlan.PlanCompleteBehavior != PlanCompleteBehavior.Restart_plan))
                                        {
                                            P.DebugLog($"Completed and behavior is {adata.VenturePlan.PlanCompleteBehavior}");
                                            TaskCollectVenture.Enqueue();
                                            if (adata.VenturePlan.PlanCompleteBehavior == PlanCompleteBehavior.Assign_Quick_Venture)
                                            {
                                                P.DebugLog($"Assigning quick venture");
                                                TaskAssignQuickVenture.Enqueue();
                                            }
                                            adata.EnablePlanner = false;
                                            P.DebugLog($"Now disabling planner");
                                        }
                                        else
                                        {
                                            ret.ProcessVenturePlanner(next);
                                        }
                                        if (completed)
                                        {
                                            adata.VenturePlanIndex = 0;
                                        }
                                        adata.VenturePlanIndex++;
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
                                            //P.DebugLog($"Behavior: {behavior}");
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
                var adata = Utils.GetAdditionalData(Svc.ClientState.LocalContentId, rname);
                if (P.GetSelectedRetainers(Svc.ClientState.LocalContentId).Contains(rname)
                    && r.GetVentureSecondsRemaining() <= P.config.UnsyncCompensation && (r.VentureID != 0 || P.config.EnableAssigningQuickExploration || (adata.EnablePlanner && adata.VenturePlan.ListUnwrapped.Count > 0)))
                {
                    return rname;
                }
            }
        }
        return null;
    }
}
