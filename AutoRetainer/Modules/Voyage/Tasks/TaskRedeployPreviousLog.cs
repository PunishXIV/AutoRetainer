using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskRedeployPreviousLog
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"Task enqueued: {nameof(TaskRedeployPreviousLog)}");
        TaskIntelligentRepair.Enqueue(name, type);
        P.TaskManager.Enqueue(VoyageScheduler.SelectViewPreviousLog);
        P.TaskManager.Enqueue(VoyageScheduler.WaitUntilFinalizeDeployAddonExists);
        P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
        P.TaskManager.EnqueueDelay(10, true);
        P.TaskManager.Enqueue(CheckForFuel);
        P.TaskManager.Enqueue(VoyageScheduler.DeployVessel);
        P.TaskManager.Enqueue(VoyageScheduler.WaitForCutscene);
        P.TaskManager.Enqueue(VoyageScheduler.WaitForNoCutscene);
    }

    internal static bool? CheckForFuel()
    {
        if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out var addon) && IsAddonReady(addon))
        {
            var fuel = addon->AtkValues[1].String;
            if(fuel != null)
            {
                var values = MemoryHelper.ReadStringNullTerminated((nint)fuel.Value).Split("/");
                if(values.Length == 2 && uint.TryParse(values[0], out var req) && uint.TryParse(values[1], out var have))
                {
                    if(req > have)
                    {
                        P.TaskManager.Abort();
                        DuoLog.Warning($"[Voyage] You are out of fuel!");
                        if(C.FailureNoFuel == WorkshopFailAction.ExcludeChar)
                        {
                            Data.WorkshopEnabled = false;
                        }
                        else if(C.FailureNoFuel == WorkshopFailAction.StopPlugin)
                        {
                            MultiMode.Enabled = false;
                            VoyageScheduler.Enabled = false;
                        }
                        P.TaskManager.BeginStack();
                        P.TaskManager.Enqueue(VoyageScheduler.CancelDeployVessel);
                        P.TaskManager.Enqueue(VoyageScheduler.WaitUntilFinalizeDeployAddonExists);
                        P.TaskManager.Enqueue(VoyageScheduler.FinalizeVessel);
                        P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
                        P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselSelectorMenu);
                        P.TaskManager.Enqueue(VoyageScheduler.SelectExitMainPanel);
                        P.TaskManager.InsertStack();
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
