using AutoRetainer.Internal;
using ECommons.Throttlers;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal unsafe static class TaskRepairAll
    {
        internal static volatile bool Abort = false;
        internal static string Name = "";
        internal static VoyageType Type = 0;
        internal static void EnqueueImmediate(List<int> indexes, string vesselName, VoyageType type)
        {
            Name = vesselName;
            Type = type;
            Abort = false;
            var vesselIndex = VoyageUtils.GetVesselIndexByName(vesselName, type);
            P.TaskManager.EnqueueImmediate(() => Utils.TrySelectSpecificEntry(new string[] { "Repair submersible components" , "Repair airship components"}, () => EzThrottler.Throttle("RepairAllSelectRepair")), "RepairAllSelectRepair");
            foreach(int index in indexes)
            {
                if(index < 0 || index > 3) throw new ArgumentOutOfRangeException(nameof(index));
                P.TaskManager.EnqueueImmediate(() => VoyageScheduler.TryRepair(index), $"Repair {index}");
                P.TaskManager.EnqueueImmediate(VoyageScheduler.ConfirmRepair, 5000, false);
                P.TaskManager.EnqueueImmediate(() => Abort || VoyageScheduler.WaitForYesNoDisappear() == true, 5000, false, "WaitForYesNoDisappear");
                P.TaskManager.EnqueueImmediate(() => Abort || VoyageUtils.GetVesselComponent(vesselIndex, type, index)->Condition > 0, "WaitUntilRepairComplete");
                P.TaskManager.DelayNextImmediate(5, true);
            }
            P.TaskManager.EnqueueImmediate(VoyageScheduler.CloseRepair);
            //P.TaskManager.Enqueue(() => Abort ? VoyageScheduler.SelectQuitVesselMenu() : true, "SelectQuitVesselMenu failed repair");
        }
    }
}
