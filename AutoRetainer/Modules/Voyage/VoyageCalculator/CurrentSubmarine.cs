using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

internal static unsafe class CurrentSubmarine
{
    internal static ExcelSheet<SubmarineExplorationPretty> ExplorationSheet => Svc.Data.GetExcelSheet<SubmarineExplorationPretty>();
    internal static HousingWorkshopSubmersibleSubData* Get()
    {
        var cur = HousingManager.Instance()->WorkshopTerritory->Submersible.DataPointers[4];
        return cur.Value;
    }

    public static List<uint> GetUnlockedSectors()
    {
        var ret = new List<uint>();
        foreach (var submarineExploration in Svc.Data.GetExcelSheet<SubmarineExploration>())
        {
            if (HousingManager.IsSubmarineExplorationUnlocked((byte)submarineExploration.RowId)) ret.Add(submarineExploration.RowId);
        }
        return ret;
    }

    public static List<uint> GetExploredSectors()
    {
        var ret = new List<uint>();
        foreach (var submarineExploration in Svc.Data.GetExcelSheet<SubmarineExploration>())
        {
            if (HousingManager.IsSubmarineExplorationExplored((byte)submarineExploration.RowId)) ret.Add(submarineExploration.RowId);
        }
        return ret;
    }

    public static uint[] GetMaps()
    {
        var maps = ExplorationSheet
                       .Where(r => r.StartingPoint)
                       .Select(r => ExplorationSheet.GetRow(r.RowId + 1)!)
                       .Where(r => r.RankReq <= Get()->RankId)
                       .Where(r => GetUnlockedSectors().Contains(r.RowId))
                       .Select(r => r.Map.Value!.RowId)
                       .ToArray();
        return maps;
    }

    public static void GetBestExps()
    {
        var calc = new Calculator();
        var maps = GetMaps();
        Task.Run(() =>
        {
            VoyageMain.WaitOverlay.IsProcessing = true;
            foreach (var x in maps)
            {
                calc.RouteBuild.Value.ChangeMap((int)x);
                var best = calc.FindBestPath(x);
                if (best != null)
                {
                    DuoLog.Information($"Map {x}: {best.Value.path.Select(z => $"{z}/{ExplorationSheet.GetRow(z).Location}").Print()}, {best.Value.duration}, {best.Value.exp} / ");
                }
            }
            VoyageMain.WaitOverlay.IsProcessing = false;
        });
    }

    public static void Fill()
    {
        return;
        var calc = new Calculator();
        Task.Run(() =>
        {
            VoyageMain.WaitOverlay.IsProcessing = true;
            calc.RouteBuild.Value.ChangeMap((int)1);
            var best = calc.FindBestPath(1);
            if (best != null)
            {
                DuoLog.Information($"{best.Value.path.Select(z => $"{z}/{ExplorationSheet.GetRow(z).Location}").Print()}, {best.Value.duration}, {best.Value.exp}");
                new TickScheduler(delegate
                {
                    foreach (var x in best.Value.path)
                    {
                        P.TaskManager.Enqueue(() => P.Memory.SelectRoutePointUnsafe((int)(x)));
                    }
                });
            }
            VoyageMain.WaitOverlay.IsProcessing = false;
        });
    }
}
