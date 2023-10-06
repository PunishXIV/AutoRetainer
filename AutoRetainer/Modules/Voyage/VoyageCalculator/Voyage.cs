using Lumina.Excel.GeneratedSheets;
using Lumina.Excel;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

public static class Voyage
{
    private const int FixedVoyageTime = 43200; // 12h

    private static ExcelSheet<SubmarineExploration> ExplorationSheet = null!;
    private static List<uint> ReversedStartPoints = null!;

    public static void Initialize()
    {
        ExplorationSheet = Svc.Data.GetExcelSheet<SubmarineExploration>();
        ReversedStartPoints = ExplorationSheet.Where(s => s.StartingPoint).Select(s => s.RowId).Reverse().ToList();
    }

    public static uint SectorToMap(uint sector)
    {
        return ExplorationSheet.GetRow(FindVoyageStartPoint(sector))!.Map.Row;
    }

    public static uint FindVoyageStartPoint(uint point)
    {
        // This works because we reversed the list of start points
        foreach (var possibleStart in ReversedStartPoints)
            if (point > possibleStart)
                return possibleStart;

        return 0;
    }

    #region Optimizer
    public static uint CalculateDuration(IEnumerable<SubmarineExplorationPretty> walkingPoints, Build.SubmarineBuild build)
    {
        var walkWay = walkingPoints.ToArray();
        var start = walkWay.First();

        var points = new List<SubmarineExplorationPretty>();
        foreach (var p in walkWay.Skip(1))
            points.Add(p);

        switch (points.Count)
        {
            case 0:
                return 0;
            case 1: // 1 point makes no sense to optimize, so just return distance
                {
                    var onlyPoint = points[0];
                    return VoyageTime(start, onlyPoint, (short)build.Speed) + SurveyTime(onlyPoint, (short)build.Speed) + FixedVoyageTime;
                }
            case > 5: // More than 5 points isn't allowed ingame
                return 0;
        }

        var allDurations = new List<long>();
        for (var i = 0; i < points.Count; i++)
        {
            var voyage = i == 0
                             ? VoyageTime(start, points[0], (short)build.Speed)
                             : VoyageTime(points[i - 1], points[i], (short)build.Speed);
            var survey = SurveyTime(points[i], (short)build.Speed);
            allDurations.Add(voyage + survey);
        }

        return (uint)allDurations.Sum() + FixedVoyageTime;
    }

    public static (int Distance, List<SubmarineExplorationPretty> Points) CalculateDistance(IEnumerable<SubmarineExplorationPretty> walkingPoints)
    {
        var walkWay = walkingPoints.ToArray();
        var start = walkWay.First();

        var points = new List<SubmarineExplorationPretty>();
        foreach (var p in walkWay.Skip(1))
            points.Add(p);


        // zero
        if (points.Count == 0)
            return (0, new List<SubmarineExplorationPretty>());

        // 1 point makes no sense to optimize, so just return distance
        if (points.Count == 1)
        {
            var onlyPoint = points[0];
            var distance = BestDistance(start, onlyPoint) + onlyPoint.SurveyDistance;
            return ((int)distance, new List<SubmarineExplorationPretty> { onlyPoint });
        }

        // More than 5 points isn't allowed ingame
        if (points.Count > 5)
            return (0, new List<SubmarineExplorationPretty>());

        List<(SubmarineExplorationPretty Key, uint Start, Dictionary<uint, uint> Distances)> AllDis = new();
        foreach (var (point, idx) in points.Select((val, i) => (val, i)))
        {
            AllDis.Add((point, BestDistance(start, point), new()));

            foreach (var iPoint in points)
            {
                if (point.RowId == iPoint.RowId)
                    continue;

                AllDis[idx].Distances.Add(iPoint.RowId, BestDistance(point, iPoint));
            }
        }

        List<(uint Way, List<SubmarineExplorationPretty> Points)> MinimalWays = new List<(uint Way, List<SubmarineExplorationPretty> Points)>();
        try
        {
            foreach (var (point, idx) in AllDis.Select((val, i) => (val, i)))
            {
                var otherPoints = AllDis.ToList();
                otherPoints.RemoveAt(idx);

                var others = new Dictionary<uint, Dictionary<uint, uint>>();
                foreach (var p in otherPoints)
                {
                    var listDis = new Dictionary<uint, uint>();
                    foreach (var dis in p.Distances)
                    {
                        listDis.Add(points.First(t => t.RowId == dis.Key).RowId, dis.Value);
                    }

                    others[p.Key.RowId] = listDis;
                }

                MinimalWays.Add(PathWalker(point, others, walkWay));
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e.Message);
            PluginLog.Error(e.StackTrace!);
        }

        var min = MinimalWays.MinBy(m => m.Way);
        var surveyD = min.Points.Sum(d => d.SurveyDistance);
        return ((int)min.Way + surveyD, min.Points);
    }

    public static (uint Distance, List<SubmarineExplorationPretty> Points) PathWalker((SubmarineExplorationPretty Key, uint Start, Dictionary<uint, uint> Distances) point, Dictionary<uint, Dictionary<uint, uint>> otherPoints, SubmarineExplorationPretty[] allPoints)
    {
        List<(uint Distance, List<SubmarineExplorationPretty> Points)> possibleDistances = new();
        foreach (var pos1 in otherPoints)
        {
            if (point.Key.RowId == pos1.Key)
                continue;

            var startToFirst = point.Start + point.Distances[pos1.Key];

            if (otherPoints.Count == 1)
            {
                possibleDistances.Add((startToFirst, new List<SubmarineExplorationPretty> { point.Key, allPoints.First(t => t.RowId == pos1.Key), }));
                continue;
            }

            foreach (var pos2 in otherPoints)
            {
                if (pos1.Key == pos2.Key || point.Key.RowId == pos2.Key)
                    continue;

                var startToSecond = startToFirst + otherPoints[pos1.Key][pos2.Key];

                if (otherPoints.Count == 2)
                {
                    possibleDistances.Add((startToSecond, new List<SubmarineExplorationPretty> { point.Key, allPoints.First(t => t.RowId == pos1.Key), allPoints.First(t => t.RowId == pos2.Key), }));
                    continue;
                }

                foreach (var pos3 in otherPoints)
                {
                    if (pos1.Key == pos3.Key || pos2.Key == pos3.Key || point.Key.RowId == pos3.Key)
                        continue;

                    var startToThird = startToSecond + otherPoints[pos2.Key][pos3.Key];

                    if (otherPoints.Count == 3)
                    {
                        possibleDistances.Add((startToThird, new List<SubmarineExplorationPretty> { point.Key, allPoints.First(t => t.RowId == pos1.Key), allPoints.First(t => t.RowId == pos2.Key), allPoints.First(t => t.RowId == pos3.Key), }));
                        continue;
                    }

                    foreach (var pos4 in otherPoints)
                    {
                        if (pos1.Key == pos4.Key || pos2.Key == pos4.Key || pos3.Key == pos4.Key || point.Key.RowId == pos4.Key)
                            continue;

                        var startToLast = startToThird + otherPoints[pos3.Key][pos4.Key];

                        possibleDistances.Add((startToLast, new List<SubmarineExplorationPretty> { point.Key, allPoints.First(t => t.RowId == pos1.Key), allPoints.First(t => t.RowId == pos2.Key), allPoints.First(t => t.RowId == pos3.Key), allPoints.First(t => t.RowId == pos4.Key), }));
                    }
                }
            }
        }

        return possibleDistances.MinBy(a => a.Distance);
    }

    public static uint BestDistance(SubmarineExplorationPretty pointA, SubmarineExplorationPretty pointB)
    {
        return pointA.GetDistance(pointB);
    }

    public static uint VoyageTime(SubmarineExplorationPretty pointA, SubmarineExplorationPretty pointB, short speed)
    {
        return pointA.GetVoyageTime(pointB, speed);
    }

    public static uint SurveyTime(SubmarineExplorationPretty point, short speed)
    {
        return point.GetSurveyTime(speed);
    }
    #endregion
}
