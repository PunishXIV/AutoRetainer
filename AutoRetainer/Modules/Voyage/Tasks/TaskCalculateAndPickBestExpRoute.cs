using AutoRetainer.Modules.Voyage.VoyageCalculator;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Modules.Voyage.Tasks
{
    internal static unsafe class TaskCalculateAndPickBestExpRoute
    {
        static volatile bool Calculating = false;
        internal static void Enqueue()
        {
            P.TaskManager.Enqueue(Calculate);
            P.TaskManager.Enqueue(WaitUntilCalculationStopped);
        }

        internal static void Calculate()
        {
            Calculating = true;
            var calc = new Calculator();
            Task.Run(() =>
            {
                VoyageMain.WaitOverlay.IsProcessing = true;
                double exp = 0;
                uint[] path = null;
                var selectedMap = 0;
                foreach (var map in CurrentSubmarine.GetMaps())
                {
                    calc.RouteBuild.Value.ChangeMap((int)map);
                    var best = calc.FindBestPath(map);
                    if (best != null)
                    {
                        VoyageUtils.Log($"{best.Value.path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(z).Location}").Print()}, {best.Value.duration}, {best.Value.exp}");
                        if(best.Value.exp > exp) 
                        {
                            selectedMap = (int)map;
                            exp = best.Value.exp;
                            path = best.Value.path;
                        }
                    }
                }
                if(path != null)
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
