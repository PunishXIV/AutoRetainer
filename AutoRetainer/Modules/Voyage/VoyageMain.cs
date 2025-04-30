﻿using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuizmoNET;
using System.Numerics;
using System.Xml.Linq;

namespace AutoRetainer.Modules.Voyage;

internal static unsafe class VoyageMain
{
    private static bool IsInVoyagePanel = false;

    internal static WaitOverlay WaitOverlay;

    internal static void Init()
    {
        Svc.Framework.Update += Tick;
        Svc.Toasts.ErrorToast += Toasts_ErrorToast;
        WaitOverlay = new();
        P.WindowSystem.AddWindow(WaitOverlay);
    }

    private static void Toasts_ErrorToast(ref SeString message, ref bool isHandled)
    {
        if(MultiMode.Active || P.TaskManager.IsBusy)
        {
            var txt = message.GetText();
            if(txt == Lang.VoyageInventoryError)
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
                else if(C.FailureNoInventory == WorkshopFailAction.ExcludeChar)
                {
                    Data.WorkshopEnabled = false;
                }
            }
            if(txt.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.UnableToRepairVessel))
            {
                TaskRepairAll.Abort = true;
                DuoLog.Warning($"[Voyage] You are out of repair components!");
                if(C.FailureNoRepair == WorkshopFailAction.ExcludeVessel)
                {
                    Data.GetEnabledVesselsData(TaskRepairAll.Type).Remove(TaskRepairAll.Name);
                }
                else if(C.FailureNoRepair == WorkshopFailAction.ExcludeChar)
                {
                    Data.WorkshopEnabled = false;
                }
                else if(C.FailureNoRepair == WorkshopFailAction.StopPlugin)
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
    }

    internal static void Tick(object _)
    {
        if(VoyageUtils.IsVoyageCondition())
        {
            if(Svc.Targets.Target.IsVoyagePanel())
            {
                if(!IsInVoyagePanel)
                {
                    PluginLog.Debug($"Entered voyage panel");
                    IsInVoyagePanel = true;
                    //Notify.Info($"Entered voyage panel");
                    if(IsKeyPressed(C.Suppress))
                    {
                        Notify.Warning("No operation was requested by user");
                    }
                    else
                    {
                        if(C.SubsAutoResend2)
                        {
                            if(Data.AnyEnabledVesselsAvailable())
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
            if(IsInVoyagePanel)
            {
                IsInVoyagePanel = false;
                //Notify.Info("Closed voyage panel");
                VoyageScheduler.Enabled = false;
                PluginLog.Debug($"<!> Exited voyage panel, disabled voyage scheduler");
            }
        }

        if(VoyageUtils.IsInVoyagePanel())
        {
            if(EzThrottler.Throttle("Voyage.WriteOfflineData", 100))
            {
                VoyageUtils.WriteOfflineData();
            }
        }

        if(VoyageScheduler.Enabled)
        {
            DoWorkshopPanelTick();
        }
    }

    private static void DoWorkshopPanelTick()
    {
        if(!P.TaskManager.IsBusy)
        {
            if(FrameThrottler.Check("SchedulerRestartCooldown"))
            {
                var data = Data;
                var panel = VoyageUtils.GetCurrentWorkshopPanelType();
                if(panel == PanelType.TypeSelector)
                {
                    if(data.AnyEnabledVesselsAvailable(VoyageType.Airship))
                    {
                        if(EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.SelectAirshipManagement);
                        }
                    }
                    else if(data.AnyEnabledVesselsAvailable(VoyageType.Submersible))
                    {
                        if(EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.SelectSubManagement);
                        }
                    }
                    else if(!data.AreAnyEnabledVesselsReturnInNext(5 * 60))
                    {
                        if(EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.SelectExitMainPanel);
                        }
                    }
                }
                else if(panel == PanelType.Submersible)
                {
                    ScheduleResend(VoyageType.Submersible);
                }
                else if(panel == PanelType.Airship)
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

    private static void ScheduleResend(VoyageType type)
    {
        var next = VoyageUtils.GetNextCompletedVessel(type);
        if(next != null)
        {
            var adata = Data.GetAdditionalVesselData(next, type);
            var data = Data.GetOfflineVesselData(next, type) ?? throw new NullReferenceException($"Offline vessel data for {next}, {type} is null");
            if((VoyageUtils.DontReassign || adata.VesselBehavior == VesselBehavior.Finalize || (C.FinalizeBeforeResend && Data.AreAnyEnabledVesselsReturnInNext(0, false, true))) && data.ReturnTime != 0)
            {
                if(EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                {
                    TaskFinalizeVessel.Enqueue(next, type, true);
                }
            }
            else
            {
                if(adata.VesselBehavior.EqualsAny(VesselBehavior.LevelUp, VesselBehavior.Unlock, VesselBehavior.Use_plan, VesselBehavior.Redeploy))
                {
                    if(EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                    {
                        if(data.ReturnTime != 0)
                        {
                            TaskFinalizeVessel.Enqueue(next, type, false);
                        }
                        else
                        {
                            TaskSelectVesselByName.Enqueue(next, type);
                        }

                        if (C.EnableAutomaticComponentsAndPlanChange)
                        {
                            TaskIntelligentComponentsChange.Enqueue(next, type);

                            var plan = VoyageUtils.GetPlanInLevelRange(Data.GetAdditionalVesselData(next, type).Level);

                            if(VoyageUtils.GetIsVesselNeedsPartsSwap(next, VoyageType.Submersible, out _).Count == 0)
                                if (plan.FirstSubDifferent && VoyageUtils.GetVesselIndexByName(next, VoyageType.Submersible) == 0)
                                {
                                    Data.AdditionalSubmarineData[next].UnlockMode = plan.FirstSubUnlockMode;
                                    Data.AdditionalSubmarineData[next].SelectedUnlockPlan = plan.FirstSubSelectedUnlockPlan;
                                    Data.AdditionalSubmarineData[next].VesselBehavior = plan.FirstSubVesselBehavior;
                                    Data.AdditionalSubmarineData[next].SelectedPointPlan = plan.FirstSubSelectedPointPlan;
                                }
                                else
                                {
                                    Data.AdditionalSubmarineData[next].UnlockMode = plan.UnlockMode;
                                    Data.AdditionalSubmarineData[next].SelectedUnlockPlan = plan.SelectedUnlockPlan;
                                    Data.AdditionalSubmarineData[next].VesselBehavior = plan.VesselBehavior;
                                    Data.AdditionalSubmarineData[next].SelectedPointPlan = plan.SelectedPointPlan;
                                }
                        }


                        P.TaskManager.EnqueueMulti(
                            new(() => CurrentSubmarine.Get() != null),
                            new(() =>
                            {
                                P.TaskManager.BeginStack();
                                try
                                {
                                    foreach(var x in C.SubmarineUnlockPlans)
                                    {
                                        if(x.EnforcePlan)
                                        {
                                            PluginLog.Information($"Unlock plan {x.Name} is set as enforced");
                                            if(TaskDeployOnUnlockRoute.GetUnlockPointsFromPlan(x, UnlockMode.SpamOne).TryGetFirst(out var unlockPoint))
                                            {
                                                PluginLog.Information($"Enforcing plan {x.Name} on current submarine");
                                                TaskDeployOnUnlockRoute.Enqueue(next, type, x, UnlockMode.SpamOne);
                                                goto EndTask;
                                            }
                                        }
                                    }
                                    if(adata.VesselBehavior == VesselBehavior.LevelUp)
                                    {
                                        TaskDeployOnBestExpVoyage.Enqueue(next, type);
                                    }
                                    else if(adata.VesselBehavior == VesselBehavior.Unlock)
                                    {
                                        var mode = adata.UnlockMode;
                                        var plan = VoyageUtils.GetSubmarineUnlockPlanByGuid(adata.SelectedUnlockPlan) ?? VoyageUtils.GetDefaultSubmarineUnlockPlan();
                                        if(plan.EnforceDSSSinglePoint && TaskDeployOnUnlockRoute.GetUnlockPointsFromPlan(plan, UnlockMode.SpamOne).TryGetFirst(out var unlockPoint) && VoyageUtils.GetSubmarineExploration(unlockPoint.point).Value.Map.RowId == 1)
                                        {
                                            PluginLog.Information($"Override unlock mode to {UnlockMode.SpamOne}");
                                            mode = UnlockMode.SpamOne;
                                        }
                                        if(mode == UnlockMode.WhileLevelling)
                                        {
                                            TaskDeployOnBestExpVoyage.Enqueue(next, type, plan);
                                        }
                                        else if(mode.EqualsAny(UnlockMode.SpamOne, UnlockMode.MultiSelect))
                                        {
                                            TaskDeployOnUnlockRoute.Enqueue(next, type, plan, mode);
                                        }
                                        else
                                        {
                                            throw new ArgumentOutOfRangeException(nameof(mode));
                                        }
                                    }
                                    else if(adata.VesselBehavior == VesselBehavior.Use_plan)
                                    {
                                        var plan = VoyageUtils.GetSubmarinePointPlanByGuid(adata.SelectedPointPlan);
                                        if(plan != null && plan.Points.Count >= 1 && plan.Points.Count <= 5)
                                        {
                                            var current = CurrentSubmarine.Get()->CurrentExplorationPoints.ToArray().Select(x => (uint)x).Where(x => x != 0);
                                            if(!current.SequenceEqual(plan.Points))
                                            {
                                                TaskDeployOnPointPlan.Enqueue(next, type, plan);
                                            }
                                            else
                                            {
                                                TaskRedeployVessel.Enqueue(next, type);
                                            }
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
                                catch(Exception e)
                                {
                                    e.Log();
                                }
                                EndTask:
                                P.TaskManager.InsertStack();
                            })
                        );

                    }
                }
            }
        }
        else
        {
            var neededParts = new[] { (uint)Hull.Shark, (uint)Stern.Shark, (uint)Bow.Shark, (uint)Bridge.Shark };
            if (C.EnableAutomaticSubRegistration && Data.AdditionalSubmarineData.Count < Data.NumSubSlots && neededParts.All(part => InventoryManager.Instance()->GetInventoryItemCount((uint)part) > 0) && InventoryManager.Instance()->GetInventoryItemCount((uint)Items.DiveCredits) >= (2 * Data.NumSubSlots) - 1)
            {
                P.TaskManager.Enqueue(VoyageScheduler.SelectRegisterSub);
                if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResendNewSubs", 1000))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        int slot = i;
                        P.TaskManager.Enqueue(() => VoyageScheduler.ChangeComponent(slot, neededParts[slot]), $"ChangeTo{neededParts[slot]}");
                    }

                    P.TaskManager.Enqueue(VoyageScheduler.RegisterSub);
                    P.TaskManager.Enqueue(VoyageScheduler.SetupNewSub);
                    P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
                }
            }
            else if (!Data.AreAnyEnabledVesselsReturnInNext(type, 1 * 60))
            {
                if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResendQuitPanel", 1000))
                {
                    TaskQuitMenu.Enqueue();
                }
            }
        }
    }
}
