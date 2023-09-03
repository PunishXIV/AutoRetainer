using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoRetainer.Modules.Voyage.VoyageCalculator.Build;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator
{
    internal unsafe class Calculator
    {
        public List<SubmarineExplorationPretty> MustInclude = new();

        private uint[] BestPath = Array.Empty<uint>();
        private List<SubmarineExplorationPretty> AllowedSectors = new();

        Lumina.Excel.ExcelSheet<SubmarineExplorationPretty> ExplorationSheet => Svc.Data.GetExcelSheet<SubmarineExplorationPretty>();

        internal SubmarineBuild? CurrentBuild;
        internal RouteBuild? RouteBuild;

        internal Calculator()
        {
            var current = CurrentSubmarine.Get();
            CurrentBuild = new(current->RankId, current->HullId, current->SternId, current->BowId, current->BridgeId);
            RouteBuild = new(current->RankId, current->HullId, current->SternId, current->BowId, current->BridgeId);
        }

        public (uint[] path, TimeSpan duration, double exp)? FindBestPath(uint mapId)
        {
            try
            {
                var routeBuild = RouteBuild.Value;
                if (CurrentBuild != null)
                {
                    DuoLog.Debug($"Starting to get best path for map {mapId}");
                    var mapDictionary = new ConcurrentDictionary<int, (int, List<SubmarineExplorationPretty>)[]>();

                    List<SubmarineExplorationPretty> valid;
                    int highestRank;
                    try
                    {
                        valid = ExplorationSheet
                                .Where(r => r.Map.Row == mapId && !r.StartingPoint && r.RankReq <= routeBuild.Rank)
                                .Where(r => CurrentSubmarine.GetUnlockedSectors().Contains(r.RowId))
                                .ToList();
                        if (AllowedSectors.Any())
                        {
                            valid = ExplorationSheet
                                    .Where(r => r.Map.Row == mapId && !r.StartingPoint && r.RankReq <= routeBuild.Rank)
                                    .Where(r => AllowedSectors.Contains(r))
                                    .ToList();
                        }
                        highestRank = valid.Max(r => r.RankReq);
                    }
                    catch (KeyNotFoundException)
                    {
                        return null;
                    }

                    var startPoint = ExplorationSheet.First(r => r.Map.Row == mapId);
                    if (!mapDictionary.TryGetValue(highestRank, out var distances) || !distances.Any(t => t.Item2.ContainsAllItems(MustInclude)))
                    {
                        var paths = valid.Select(t => new[] { startPoint.RowId, t.RowId }.ToList()).ToHashSet(new ListComparer());
                        if (MustInclude.Any())
                            paths = new[] { MustInclude.Select(t => t.RowId).Prepend(startPoint.RowId).ToList() }.ToHashSet(new ListComparer());

                        var i = MustInclude.Any() ? MustInclude.Count : 1;
                        while (i++ < 5)
                        {
                            foreach (var path in paths.ToArray())
                            {
                                foreach (var validPoint in valid.Where(t => !path.Contains(t.RowId)))
                                {
                                    var pathNew = path.ToList();
                                    pathNew.Add(validPoint.RowId);
                                    paths.Add(pathNew.ToList());
                                }
                            }
                        }

                        var allPaths = paths.AsParallel().Select(t => t.Select(f => valid.FirstOrDefault(k => k.RowId == f) ?? startPoint)).ToList();

                        if (!allPaths.Any())
                        {
                            return null;
                        }

                        distances = allPaths.AsParallel().Select(Voyage.CalculateDistance).ToArray();
                        mapDictionary.AddOrUpdate(highestRank, distances, (k, v) => distances);
                    }

                    var build = routeBuild.GetSubmarineBuild;
                    var optimalDistances = distances.AsParallel().Where(t => t.Item1 <= build.Range && t.Item2.ContainsAllItems(MustInclude)).ToArray();
                    if (!optimalDistances.Any())
                    {
                        return null;
                    }

                    var bestPath = optimalDistances.Select(t =>
                    {
                        var path = t.Item2.Prepend(startPoint).ToArray();

                        return new Tuple<uint[], TimeSpan, double>(
                            t.Item2.Select(t => t.RowId).ToArray(),
                            TimeSpan.FromSeconds(Voyage.CalculateDuration(path, build)),
                            Sectors.CalculateExpForSectors(t.Item2, CurrentBuild.Value)
                        );
                    })
                      //.Where(t => t.Item2 < Configuration.DurationLimit.ToTime())
                      .OrderByDescending(t => t.Item3 / t.Item2.TotalMinutes)
                      //.Select(t => t.Item1)
                      .FirstOrDefault();

                    if (bestPath == null)
                    {
                        return null;
                    }
                    return (bestPath.Item1, bestPath.Item2, bestPath.Item3);
                }
            }
            catch(Exception e)
            {
                PluginLog.Error($"Error calculating best path");
                e.Log();
            }
            return null;
        }
    }
}
