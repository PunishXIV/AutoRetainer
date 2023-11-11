using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.Throttlers;

namespace AutoRetainer.Modules.Voyage;

internal static unsafe class VoyageMain
{
    static bool IsInVoyagePanel = false;

    internal static WaitOverlay WaitOverlay;

    internal static void Init()
    {
        Svc.Framework.Update += Tick;
        Svc.Toasts.ErrorToast += Toasts_ErrorToast;
        VoyageMemory.Init();
        WaitOverlay = new();
        P.ws.AddWindow(WaitOverlay);
    }

    private static void Toasts_ErrorToast(ref SeString message, ref bool isHandled)
    {
        if (MultiMode.Active || P.TaskManager.IsBusy)
        {
            var txt = message.ExtractText();
            if (txt == Lang.VoyageInventoryError)
            {
                DuoLog.Warning($"[Voyage] Your inventory is full!");
                VoyageScheduler.Enabled = false;
                P.TaskManager.Abort();
                P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselSelectorMenu);
                P.TaskManager.Enqueue(VoyageScheduler.SelectExitMainPanel);
                if(C.FailureNoInventory == WorkshopFailAction.StopPlugin)
                {
                    MultiMode.Enabled = false;
                    VoyageScheduler.Enabled = false;
                }
                else if (C.FailureNoInventory == WorkshopFailAction.ExcludeChar)
                {
                    Data.WorkshopEnabled = false;
                }
            }
            if (txt.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.UnableToRepairVessel))
            {
                TaskRepairAll.Abort = true;
                DuoLog.Warning($"[Voyage] You are out of repair components!");
                if (C.FailureNoRepair == WorkshopFailAction.ExcludeVessel)
                {
                    Data.GetEnabledVesselsData(TaskRepairAll.Type).Remove(TaskRepairAll.Name);
                }
                else if (C.FailureNoRepair == WorkshopFailAction.ExcludeChar)
                {
                    Data.WorkshopEnabled = false;
                }
                else if (C.FailureNoRepair == WorkshopFailAction.StopPlugin)
                {
                    MultiMode.Enabled = false;
                    VoyageScheduler.Enabled = false;
                }
            }
        }
    }

    internal static void Shutdown()
    {
        Svc.Framework.Update -= Tick;
        Svc.Toasts.ErrorToast -= Toasts_ErrorToast;
        VoyageMemory.Dispose();
    }

    internal static void Tick(object _)
    {
        if (VoyageUtils.IsVoyageCondition())
        {
            if (Svc.Targets.Target.IsVoyagePanel())
            {
                if (!IsInVoyagePanel)
                {
                    PluginLog.Debug($"Entered voyage panel");
                    IsInVoyagePanel = true;
                    Notify.Info($"Entered voyage panel");
                    if (IsKeyPressed(C.Suppress))
                    {
                        Notify.Warning("No operation was requested by user");
                    }
                    else
                    {
                        if (C.SubsAutoResend2)
                        {
                            if (Data.AnyEnabledVesselsAvailable())
                            {
                                VoyageScheduler.Enabled = true;
                                PluginLog.Debug($"<!> Enabled voyage scheduler");
                            }
                            else
                            {
                                Notify.Warning($"Warning!\nDeployables were not enabled as there are nothing to process yet");
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (IsInVoyagePanel)
            {
                IsInVoyagePanel = false;
                Notify.Info("Closed voyage panel");
                VoyageScheduler.Enabled = false;
                PluginLog.Debug($"<!> Exited voyage panel, disabled voyage scheduler");
            }
        }

        if (VoyageUtils.IsInVoyagePanel())
        {
            if (EzThrottler.Throttle("Voyage.WriteOfflineData", 100))
            {
                VoyageUtils.WriteOfflineData();
            }
        }

        if (VoyageScheduler.Enabled)
        {
            DoWorkshopPanelTick();
        }
    }

    static void DoWorkshopPanelTick()
    {
        if (!P.TaskManager.IsBusy)
        {
            if (FrameThrottler.Check("SchedulerRestartCooldown"))
            {
                var data = Data;
                var panel = VoyageUtils.GetCurrentWorkshopPanelType();
                if (panel == PanelType.TypeSelector)
                {
                    if (data.AnyEnabledVesselsAvailable(VoyageType.Airship))
                    {
                        if (EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.SelectAirshipManagement);
                        }
                    }
                    else if (data.AnyEnabledVesselsAvailable(VoyageType.Submersible))
                    {
                        if (EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.SelectSubManagement);
                        }
                    }
                    else if (!data.AreAnyEnabledVesselsReturnInNext(5 * 60))
                    {
                        if (EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.SelectExitMainPanel);
                        }
                    }
                }
                else if (panel == PanelType.Submersible)
                {
                    ScheduleResend(VoyageType.Submersible);
                }
                else if (panel == PanelType.Airship)
                {
                    ScheduleResend(VoyageType.Airship);
                }
            }
        }
        else
        {
            FrameThrottler.Throttle("SchedulerRestartCooldown", 10, true);
        }
    }

    static void ScheduleResend(VoyageType type)
    {
        var next = VoyageUtils.GetNextCompletedVessel(type);
        if (next != null)
        {
            var adata = Data.GetAdditionalVesselData(next, type);
            var data = Data.GetOfflineVesselData(next, type) ?? throw new NullReferenceException($"Offline vessel data for {next}, {type} is null");
            if ((VoyageUtils.DontReassign || adata.VesselBehavior == VesselBehavior.Finalize || (C.FinalizeBeforeResend && Data.AreAnyEnabledVesselsReturnInNext(0))) && data.ReturnTime != 0)
            {
                if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                {
                    TaskFinalizeVessel.Enqueue(next, type, true);
                }
            }
            else
            {
                if (adata.VesselBehavior.EqualsAny(VesselBehavior.LevelUp, VesselBehavior.Unlock, VesselBehavior.Use_plan, VesselBehavior.Redeploy))
                {
                    if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                    {
                        if (data.ReturnTime != 0)
                        {
                            TaskFinalizeVessel.Enqueue(next, type, false);
                        }
                        else
                        {
                            TaskSelectVesselByName.Enqueue(next, type);
                        }
                        if (adata.VesselBehavior == VesselBehavior.LevelUp)
                        {
                            TaskDeployOnBestExpVoyage.Enqueue(next, type);
                        }
                        else if(adata.VesselBehavior == VesselBehavior.Unlock)
                        {
                            if(adata.UnlockMode == UnlockMode.WhileLevelling)
                            {
                                TaskDeployOnBestExpVoyage.Enqueue(next, type, VoyageUtils.GetSubmarineUnlockPlanByGuid(adata.SelectedUnlockPlan) ?? new());
                            }
                            else if(adata.UnlockMode.EqualsAny(UnlockMode.SpamOne, UnlockMode.MultiSelect))
                            {
                                TaskDeployOnUnlockRoute.Enqueue(next, type, VoyageUtils.GetSubmarineUnlockPlanByGuid(adata.SelectedUnlockPlan) ?? new(), adata.UnlockMode);
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException(nameof(adata.UnlockMode));
                            }
                        }
                        else if (adata.VesselBehavior == VesselBehavior.Use_plan)
                        {
                            var plan = VoyageUtils.GetSubmarinePointPlanByGuid(adata.SelectedPointPlan);
                            if (plan != null && plan.Points.Count >= 1 && plan.Points.Count <= 5)
                            {
                                TaskDeployOnPointPlan.Enqueue(next, type, plan);
                            }
                            else
                            {
                                DuoLog.Error($"Invalid plan selected (Points.Count={plan.Points.Count})");
                            }
                        }
                        else if(adata.VesselBehavior == VesselBehavior.Redeploy)
                        {
                            TaskRedeployVessel.Enqueue(next, type);
                        }
                    }
                }
            }
        }
        else
        {
            if (!Data.AreAnyEnabledVesselsReturnInNext(type, 1 * 60))
            {
                if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResendQuitPanel", 1000))
                {
                    TaskQuitMenu.Enqueue();
                }
            }
        }
    }
}
