using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
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
            VoyageUtils.Log($"Task enqueued: {nameof(TaskCalculateAndPickBestExpRoute)}");
            P.TaskManager.Enqueue(() => Calculate(unlock));
            P.TaskManager.Enqueue(WaitUntilCalculationStopped, 60*60*1000);
        }

        internal static void Calculate(SubmarineUnlockPlan unlock)
        {
            Calculating = true;
            var calc = new Calculator();
            Task.Run(() =>
            {
                VoyageMain.WaitOverlay.IsProcessing = true;
                double exp = 0;
                uint[] path = null;
                var selectedMap = 0;
                var prioList = unlock?.GetPrioritizedPointList();
                foreach (var map in CurrentSubmarine.GetMaps())
                {
                    calc.RouteBuild.Value.ChangeMap((int)map);
                    if (prioList != null && prioList.Count > 0)
                    {
                        var point = Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(prioList[0]);
                        if (point.Map.Row != map) continue;
                        if (point.RankReq > CurrentSubmarine.Get()->RankId) continue;
                        calc.MustInclude.Add(Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(prioList[0]));
                    }
                    var best = calc.FindBestPath(map);
                    if (best != null)
                    {
                        var xptime = best.Value.exp / (double)best.Value.duration.TotalHours;
                        VoyageUtils.Log($"Path {best.Value.path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(z).Location}").Print()}, is best for map {map} with {best.Value.duration} duration and {best.Value.exp} exp ({xptime} exp/hour)");
                        if(xptime > exp) 
                        {
                            selectedMap = (int)map;
                            exp = xptime;
                            path = best.Value.path;
                        }
                    }
                }
                VoyageUtils.Log($"Path {path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(z).Location}").Print()}, is determined best on map {selectedMap} with ({exp} exp/hour)");
                if (path != null)
                {
                    new TickScheduler(delegate
                    {
                        TaskPickSubmarineRoute.EnqueueImmediate((uint)selectedMap, path);
                        Calculating = false;
                    });
                }
                VoyageMain.WaitOverlay.IsProcessing = false;
            });
        }

        internal static bool? WaitUntilCalculationStopped() => !Calculating;
    }
}
