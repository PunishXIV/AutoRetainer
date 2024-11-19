using Dalamud.Game;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Drawing.Printing;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

public static class SubmarineSheetUtils
{
    public static Vector3 Position(this SubmarineExploration Row)
    {
        return new(Row.X, Row.Y, Row.Z);
    }

    public static uint GetSurveyTime(this SubmarineExploration Row, float speed)
    {
        if(speed < 1)
            speed = 1;
        return (uint)Math.Floor(Row.SurveyDurationmin * 7000 / (speed * 100) * 60);
    }

    public static uint GetVoyageTime(this SubmarineExploration Row, SubmarineExploration other, float speed)
    {
        if(speed < 1)
            speed = 1;
        return (uint)Math.Floor(Vector3.Distance(Row.Position(), other.Position()) * 3990 / (speed * 100) * 60);
    }

    public static uint GetDistance(this SubmarineExploration Row, SubmarineExploration other)
    {
        return (uint)Math.Floor(Vector3.Distance(Row.Position(), other.Position()) * 0.035);
    }

    public static string ConvertDestination(this SubmarineExploration Row) => Utils.UpperCaseStr(Row.Destination);
    public static string FancyDestination(this SubmarineExploration Row) => $"[{Svc.Data.GetExcelSheet<SubmarineExploration>(ClientLanguage.Japanese).GetRow(Row.RowId).Location}] " + Utils.UpperCaseStr(Row.Destination);
}
