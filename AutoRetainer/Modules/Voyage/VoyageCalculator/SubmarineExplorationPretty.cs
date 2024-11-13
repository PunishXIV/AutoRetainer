using Dalamud.Game;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Drawing.Printing;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

public class SubmarineExplorationPretty
{
    public Vector3 Position { get; private set; }
    public SubmarineExploration Row { get; private set; }

    public SubmarineExplorationPretty(SubmarineExploration sheet)
    {
        sheet = Row;
        Position = new Vector3(sheet.X, sheet.Y, sheet.Z);
    }

    public static implicit operator SubmarineExplorationPretty(SubmarineExploration sheet) => new(sheet);
    
    public uint GetSurveyTime(float speed)
    {
        if(speed < 1)
            speed = 1;
        return (uint)Math.Floor(Row.SurveyDurationmin * 7000 / (speed * 100) * 60);
    }

    public uint GetVoyageTime(SubmarineExploration other, float speed)
    {
        if(speed < 1)
            speed = 1;
        return (uint)Math.Floor(Vector3.Distance(Position, ((SubmarineExplorationPretty)other).Position) * 3990 / (speed * 100) * 60);
    }

    public uint GetDistance(SubmarineExploration other)
    {
        return (uint)Math.Floor(Vector3.Distance(Position, ((SubmarineExplorationPretty)other).Position) * 0.035);
    }

    public string ConvertDestination() => Utils.UpperCaseStr(Row.Destination);
    public string FancyDestination() => $"[{Svc.Data.GetExcelSheet<SubmarineExploration>(ClientLanguage.Japanese).GetRow(Row.RowId).Location}] " + Utils.UpperCaseStr(Row.Destination);
}
