using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Tasks;
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
                                var data = Utils.GetCurrentCharacterData();
                                var air = data.OfflineAirshipData.Where(x => x.ReturnTime != 0 && x.GetRemainingSeconds() < C.UnsyncCompensation && data.EnabledAirships.Contains(x.Name));
                                var sub = data.OfflineSubmarineData.Where(x => x.ReturnTime != 0 && x.GetRemainingSeconds() < C.UnsyncCompensation && data.EnabledSubs.Contains(x.Name));
                                if (air.Count() > 0)
                                {
                                    TaskEnterMenu.Enqueue(VoyageType.Airship);
                                    foreach (var x in air)
                                    {
                                        if (C.SubsOnlyFinalize || C.DontReassign)
                                        {
                                            TaskFinalizeVessel.Enqueue(x.Name);
                                        }
                                        else
                                        {
                                            TaskRedeployVessel.Enqueue(x.Name, VoyageType.Airship);
                                        }
                                    }
                                    TaskQuitMenu.Enqueue();
                                }
                                if (sub.Count() > 0)
                                {
                                    TaskEnterMenu.Enqueue(VoyageType.Submersible);
                                    foreach (var x in sub)
                                    {
                                        if (C.SubsOnlyFinalize || C.DontReassign)
                                        {
                                            TaskFinalizeVessel.Enqueue(x.Name);
                                        }
                                        else
                                        {
                                            TaskRedeployVessel.Enqueue(x.Name, VoyageType.Submersible);
                                        }
                                    }
                                    TaskQuitMenu.Enqueue();
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
                }
            }

            if (VoyageUtils.IsInVoyagePanel())
            {
                if (EzThrottler.Throttle("Voyage.WriteOfflineData", 100))
                {
                    VoyageUtils.WriteOfflineData();
                }
            }
        }
    }
}
