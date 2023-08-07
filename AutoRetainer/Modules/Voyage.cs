using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Tasks.Voyage;
using Dalamud.Game.ClientState.Conditions;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules
{
    internal static unsafe class Voyage
    {
        static bool IsInVoyagePanel = false;

        internal static void Init()
        {
            Svc.Framework.Update += Tick;
        }

        internal static void Shutdown()
        {
            Svc.Framework.Update -= Tick;
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
                        if (C.AutoResendSubs)
                        {
                            var air = Utils.GetCurrentCharacterData().OfflineAirshipData.Where(x => x.GetRemainingSeconds() < C.UnsyncCompensation);
                            var sub = Utils.GetCurrentCharacterData().OfflineSubmarineData.Where(x => x.GetRemainingSeconds() < C.UnsyncCompensation);
                            if (air.Count() > 0)
                            {
                                TaskEnterMenu.Enqueue(VoyageType.Airship);
                                foreach (var x in air)
                                {
                                    TaskRedeployVoyage.Enqueue(x.Name);
                                }
                                TaskQuitMenu.Enqueue();
                            }
                            if (sub.Count() > 0)
                            {
                                TaskEnterMenu.Enqueue(VoyageType.Submersible);
                                foreach (var x in sub)
                                {
                                    TaskRedeployVoyage.Enqueue(x.Name);
                                }
                                TaskQuitMenu.Enqueue();
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
                }
            }

            if(VoyageUtils.IsInVoyagePanel())
            {
                if (EzThrottler.Throttle("Voyage.WriteOfflineData", 100))
                {
                    VoyageUtils.WriteOfflineData();
                }
            }
        }
    }
}
