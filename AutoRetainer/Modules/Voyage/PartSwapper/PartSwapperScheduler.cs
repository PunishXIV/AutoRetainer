using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkHistory.Delegates;

namespace AutoRetainer.Modules.Voyage.PartSwapper;
public static unsafe class PartSwapperScheduler
{
    public static void EnqueuePartSwappingIfNeeded(string next, VoyageType type)
    {
        if(C.EnableAutomaticComponentsAndPlanChange)
        {
            var plan = PartSwapperUtils.GetPlanInLevelRange(Data.GetAdditionalVesselData(next, type).Level);
            if (plan == null) return;

            TaskIntelligentComponentsChange.Enqueue(next, type);
            
            if(PartSwapperUtils.GetIsVesselNeedsPartsSwap(next, VoyageType.Submersible, out _).Count == 0)
            {
                if(plan.FirstSubDifferent && VoyageUtils.GetVesselIndexByName(next, VoyageType.Submersible) == 0)
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
        }
    }

    public static bool EnqueueSubmersibleRegistrationIfPossible()
    {
        var neededParts = new[] { (uint)Hull.Shark, (uint)Stern.Shark, (uint)Bow.Shark, (uint)Bridge.Shark };
        if(C.EnableAutomaticSubRegistration 
            && Data.AdditionalSubmarineData.Count < Data.NumSubSlots 
            && neededParts.All(part => InventoryManager.Instance()->GetInventoryItemCount((uint)part) > 0) 
            && InventoryManager.Instance()->GetInventoryItemCount((uint)Items.DiveCredits) >= (2 * Data.NumSubSlots) - 1)
        {
            P.TaskManager.Enqueue(PartSwapperTasks.SelectRegisterSub);
            if(EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResendNewSubs", 1000))
            {
                for(var i = 0; i < 4; i++)
                {
                    var slot = i;
                    P.TaskManager.Enqueue(() => PartSwapperTasks.ChangeComponent(slot, neededParts[slot]), $"ChangeTo{neededParts[slot]}");
                }

                P.TaskManager.Enqueue(PartSwapperTasks.RegisterSub);
                P.TaskManager.Enqueue(PartSwapperTasks.SetupNewSub);
                P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
