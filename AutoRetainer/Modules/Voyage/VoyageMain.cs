using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoRetainer.Modules.Voyage
{
    internal static unsafe class VoyageMain
    {
        static bool IsInVoyagePanel = false;

        internal static void Init()
        {
            Svc.Framework.Update += Tick;
            VoyageMemory.Init();
        }

        internal static void Shutdown()
        {
            Svc.Framework.Update -= Tick;
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
                        IsInVoyagePanel = true;
                        Notify.Info($"Entered voyage panel");
                        if (IsKeyPressed(C.Suppress))
                        {
                            Notify.Warning("No operation was requested by user");
                        }
                        else
                        {
                            if (C.SubsAutoResend)
                            {
                                if (Data.AnyEnabledVesselsAvailable())
                                {
                                    VoyageScheduler.Enabled = true;
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
                var data = Utils.GetCurrentCharacterData();
                var panel = VoyageUtils.GetCurrentWorkshopPanelType();
                if (panel == PanelType.TypeSelector)
                {
                    if (data.AnyEnabledVesselsAvailable(VoyageType.Airship))
                    {
                        if(EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
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
                    else if (!data.AreAnyVesselsReturnInNext(5))
                    {
                        if (EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.ExitMainPanel);
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

        static void ScheduleResend(VoyageType type)
        {
            var next = VoyageUtils.GetNextCompletedVessel(type);
            if (next != null)
            {
                if (C.SubsOnlyFinalize || C.DontReassign || Data.GetFinalizeVesselsData(type).Contains(next))
                {
                    if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                    {
                        TaskFinalizeVessel.Enqueue(next, type);
                    }
                }
                else
                {
                    if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                    {
                        TaskRedeployVessel.Enqueue(next, type);
                    }
                }
            }
            else
            {
                if (!Data.AreAnyVesselsReturnInNext(1))
                {
                    if (EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResendQuitPanel", 1000))
                    {
                        TaskQuitMenu.Enqueue();
                    }
                }
            }
        }
    }
}
