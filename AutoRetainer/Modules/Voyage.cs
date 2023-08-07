using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Tasks.Voyage;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal static class Voyage
    {
        static bool WasRun = false;
        internal static void Tick()
        {
            if (C.AutoResendSubs && Svc.Condition[ConditionFlag.OccupiedInEvent] && Svc.Targets.Target?.Name.ToString().EqualsAny(VoyageUtils.PanelName) == true) 
            {
                if (!WasRun) 
                {
                    WasRun = true;
                    if (VoyageUtils.GetCompletedAirships().Count > 0)
                    {
                        TaskEnterMenu.Enqueue(VoyageType.Airship);
                        foreach (var name in VoyageUtils.GetCompletedAirships())
                        {
                            TaskCollectVoyage.Enqueue(name);
                        }
                        TaskQuitMenu.Enqueue();
                    }
                    if (VoyageUtils.GetCompletedSubs().Count > 0)
                    {
                        TaskEnterMenu.Enqueue(VoyageType.Submersible);
                        foreach (var name in VoyageUtils.GetCompletedSubs())
                        {
                            TaskCollectVoyage.Enqueue(name);
                        }
                        TaskQuitMenu.Enqueue();
                    }
                }
            }
            else
            {
                if (WasRun)
                {
                    WasRun = false;
                }
            }
        }
    }
}
