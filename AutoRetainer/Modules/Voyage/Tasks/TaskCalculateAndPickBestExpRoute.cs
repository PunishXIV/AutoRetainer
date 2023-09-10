using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using ECommons.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static unsafe class TaskCalculateAndPickBestExpRoute
    {
        static volatile bool Calculating = false;
        internal static void Enqueue(SubmarineUnlockPlan unlock = null)
        {
            VoyageUtils.Log($"Task enqueued: {nameof(TaskCalculateAndPickBestExpRoute)} (plan: {unlock})");
            P.TaskManager.Enqueue(() => Calculate(unlock));
            P.TaskManager.Enqueue(WaitUntilCalculationStopped, 60*60*1000);
        }

        internal static void Calculate(SubmarineUnlockPlan unlock)
        {
            Calculating = true;
            var calc = new Calculator();
            var curSubMaps = CurrentSubmarine.GetMaps();
            var curSubRank = CurrentSubmarine.Get()->RankId;
            Task.Run(() =>
            {
                VoyageMain.WaitOverlay.IsProcessing = true;
                try
                {
                    double exp = 0;
                    uint[] path = null;
                    var selectedMap = 0;
                    var prioList = unlock?.GetPrioritizedPointList();
                    if(prioList != null) VoyageUtils.Log($"Prioritized point list: {prioList.Select(x => $"{VoyageUtils.GetSubmarineExplorationName(x.point)} ({x.justification})").Print()}");
                    foreach (var map in curSubMaps)
                    {
                        calc.RouteBuild.Value.ChangeMap((int)map);
                        if (prioList != null && prioList.Count > 0)
                        {
                            var point = VoyageUtils.GetSubmarineExploration(prioList[0].point);
                            if (point == null || point.Map.Row != map || point.RankReq > curSubRank)
                            {
                                //
                            }
                            else
                            {
                                VoyageUtils.Log($"Adding point: {VoyageUtils.GetSubmarineExplorationName(prioList[0].point)} ({prioList[0].justification})");
                                calc.MustInclude.Add(VoyageUtils.GetSubmarineExploration(prioList[0].point));
                            }
                        }
                        var best = calc.FindBestPath(map);
                        if (best != null && best.Value.path != null)
                        {
                            var xptime = best.Value.exp / (double)best.Value.duration.TotalHours;
                            VoyageUtils.Log($"Path {best.Value.path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(z).Location}").Print()}, is best for map {map} with {best.Value.duration} duration and {best.Value.exp} exp ({xptime} exp/hour)");
                            if (xptime > exp)
                            {
                                selectedMap = (int)map;
                                exp = xptime;
                                path = best.Value.path;
                            }
                        }
                    }
                    if (path == null) throw new Exception("Path was null.");
                    VoyageUtils.Log($"Path {path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(z).Location}").Print()}, is determined best on map {selectedMap} with ({exp} exp/hour)");
                    if (path != null)
                    {
                        new TickScheduler(delegate
                        {
                            TaskPickSubmarineRoute.EnqueueImmediate((uint)selectedMap, path);
                            Calculating = false;
                        });
                    }
                }
                catch(Exception e)
                {
                    DuoLog.Error($"Critical error occurred during path optimization: {e.Message}");
                    e.Log();
                }
                VoyageMain.WaitOverlay.IsProcessing = false;
            });
        }

        internal static bool? WaitUntilCalculationStopped() => !Calculating;
    }
}
