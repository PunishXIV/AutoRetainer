using AutoRetainerAPI.Configuration;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static unsafe class TaskRedeployPreviousLog
    {
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(VoyageScheduler.SelectViewPreviousLog);
            P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
            P.TaskManager.DelayNext(10, true);
            P.TaskManager.Enqueue(CheckForFuel);
            P.TaskManager.Enqueue(VoyageScheduler.DeployVessel);
            P.TaskManager.Enqueue(VoyageScheduler.WaitForCutscene);
            P.TaskManager.Enqueue(VoyageScheduler.PressEsc);
            P.TaskManager.Enqueue(VoyageScheduler.ConfirmSkip);
        }

        internal static bool? CheckForFuel()
        {
            if (TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon) && IsAddonReady(addon))
            {
                var fuel = addon->AtkValues[1].String;
                if (fuel != null)
                {
                    var values = MemoryHelper.ReadStringNullTerminated((nint)fuel).Split("/");
                    if (values.Length == 2 && uint.TryParse(values[0], out var req) && uint.TryParse(values[1], out var have))
                    {
                        if (req > have)
                        {
                            P.TaskManager.Abort();
                            DuoLog.Warning($"[Voyage] You are out of fuel!");
                            if (C.FailureNoFuel == WorkshopFailAction.ExcludeChar)
                            {
                                Data.WorkshopEnabled = false;
                            }
                            else if (C.FailureNoFuel == WorkshopFailAction.StopPlugin)
                            {
                                MultiMode.Enabled = false;
                                VoyageScheduler.Enabled = false;
                            }
                            P.TaskManager.EnqueueImmediate(VoyageScheduler.CancelDeployVessel);
                            P.TaskManager.EnqueueImmediate(VoyageScheduler.FinalizeVessel);
                            P.TaskManager.EnqueueImmediate(VoyageScheduler.SelectQuitVesselMenu);
                            P.TaskManager.EnqueueImmediate(VoyageScheduler.SelectQuitVesselSelectorMenu);
                            P.TaskManager.EnqueueImmediate(VoyageScheduler.SelectExitMainPanel);
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
