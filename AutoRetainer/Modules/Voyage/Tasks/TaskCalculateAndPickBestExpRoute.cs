using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskCalculateAndPickBestExpRoute
{
    private static volatile bool Calculating = false;
    internal static volatile bool Stop = false;
    internal static void Enqueue(SubmarineUnlockPlan unlock = null)
    {
        Stop = false;
        VoyageUtils.Log($"Task enqueued: {nameof(TaskCalculateAndPickBestExpRoute)} (plan: {unlock})");
        P.TaskManager.Enqueue(() => Calculate(unlock));
        P.TaskManager.Enqueue(WaitUntilCalculationStopped, new(timeLimitMS: 60 * 60 * 1000));
    }

    internal static void Calculate(SubmarineUnlockPlan unlock)
    {
        if(Stop)
        {
            Stop = false;
            return;
        }
        Calculating = true;
        var calc = new Calculator();
        var curSubMaps = CurrentSubmarine.GetMaps();
        var curSubRank = CurrentSubmarine.Get()->RankId;
        var prioList = unlock?.GetPrioritizedPointList();
        void Run()
        {
            VoyageMain.WaitOverlay.IsProcessing = true;
            try
            {
                double exp = 0;
                uint[] path = null;
                var selectedMap = 0;
                if(prioList != null) VoyageUtils.Log($"Prioritized point list: {prioList.Select(x => $"{VoyageUtils.GetSubmarineExplorationName(x.point)} ({x.justification})").Print()}");
                var calcCnt = 0;
                void Calc()
                {
                    if(calcCnt > 1) throw new Exception("Could not calculate best path.");
                    calcCnt++;
                    foreach(var map in curSubMaps)
                    {
                        calc.RouteBuild.Value.ChangeMap((int)map);
                        var doCalc = false;
                        if(prioList != null && prioList.Count > 0)
                        {
                            var point = VoyageUtils.GetSubmarineExploration(prioList[0].point);
                            if(point == null || point.Map.Row != map || point.RankReq > curSubRank)
                            {
                                //
                            }
                            else
                            {
                                doCalc = true;
                                VoyageUtils.Log($"Adding point: {VoyageUtils.GetSubmarineExplorationName(prioList[0].point)} ({prioList[0].justification})");
                                calc.MustInclude.Add(VoyageUtils.GetSubmarineExploration(prioList[0].point));
                            }
                        }
                        else
                        {
                            doCalc = true;
                        }
                        if(doCalc)
                        {
                            var best = calc.FindBestPath(map);
                            if(best != null && best.Value.path != null)
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
                        else
                        {
                            VoyageUtils.Log($"Map {map} skipped because it has no zones to unlock");
                        }
                    }
                }
                Calc();
                if(path == null)
                {
                    VoyageUtils.Log($"Path was null. Retrying without plan...");
                    calc.MustInclude.Clear();
                    prioList = null;
                    Calc();
                }
                VoyageUtils.Log($"Path {path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExplorationPretty>().GetRow(z).Location}").Print()}, is determined best on map {selectedMap} with ({exp} exp/hour)");
                if(path != null)
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
        }
        if(C.VoyageDisableCalcMultithreading)
        {
            Run();
        }
        else
        {
            Task.Run(Run);
        }
    }

    internal static bool? WaitUntilCalculationStopped() => !Calculating;
}
