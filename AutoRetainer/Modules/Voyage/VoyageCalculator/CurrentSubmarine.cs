using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

internal static unsafe class CurrentSubmarine
{
    internal static HousingWorkshopSubmersibleSubData* Get()
    {
        var cur = HousingManager.Instance()->WorkshopTerritory->Submersible.DataPointers[4];
        return cur.Value;
    }

    public static List<uint> GetUnlockedSectors()
    {
        var ret = new List<uint>();
        foreach(var submarineExploration in Svc.Data.GetExcelSheet<SubmarineExploration>())
        {
            if(HousingManager.IsSubmarineExplorationUnlocked((byte)submarineExploration.RowId)) ret.Add(submarineExploration.RowId);
        }
        return ret;
    }

    public static List<uint> GetExploredSectors()
    {
        var ret = new List<uint>();
        foreach(var submarineExploration in Svc.Data.GetExcelSheet<SubmarineExploration>())
        {
            if(HousingManager.IsSubmarineExplorationExplored((byte)submarineExploration.RowId)) ret.Add(submarineExploration.RowId);
        }
        return ret;
    }

    public static uint[] GetMaps()
    {
        var maps = Svc.Data.GetExcelSheet<SubmarineExploration>()
                       .Where(r => r.StartingPoint)
                       .Select(r => Svc.Data.GetExcelSheet<SubmarineExploration>().GetRowOrDefault(r.RowId + 1)!)
                       .Where(r => r?.RankReq <= Get()->RankId)
                       .Where(r => GetUnlockedSectors().ContainsNullable(r?.RowId))
                       .Select(r => r?.Map.Value.RowId)
                       .ToArray();
        return maps.Where(x => x != null).Select(x => x.Value).ToArray();
    }

    public static void GetBestExps()
    {
        var calc = new Calculator();
        var maps = GetMaps();
        Task.Run(() =>
        {
            VoyageMain.WaitOverlay.IsProcessing = true;
            foreach(var x in maps)
            {
                calc.RouteBuild.Value.ChangeMap((int)x);
                var best = calc.FindBestPath(x);
                if(best != null)
                {
                    DuoLog.Information($"Map {x}: {best.Value.path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExploration>().GetPretty(z).Row.Location}").Print()}, {best.Value.duration}, {best.Value.exp} / ");
                }
            }
            VoyageMain.WaitOverlay.IsProcessing = false;
        });
    }
}
