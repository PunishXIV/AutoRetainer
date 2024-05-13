using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace AutoRetainer.Modules.Voyage.VoyageCalculator;

public class SubmarineExplorationPretty : SubmarineExploration
{
		public Vector3 Position;

		public override void PopulateData(RowParser parser, GameData gameData, Language language)
		{
				base.PopulateData(parser, gameData, language);
				Position = new Vector3(X, Y, Z);
		}

		public uint GetSurveyTime(float speed)
		{
				if (speed < 1)
						speed = 1;
				return (uint)Math.Floor(SurveyDurationmin * 7000 / (speed * 100) * 60);
		}

		public uint GetVoyageTime(SubmarineExplorationPretty other, float speed)
		{
				if (speed < 1)
						speed = 1;
				return (uint)Math.Floor(Vector3.Distance(Position, other.Position) * 3990 / (speed * 100) * 60);
		}

		public uint GetDistance(SubmarineExplorationPretty other)
		{
				return (uint)Math.Floor(Vector3.Distance(Position, other.Position) * 0.035);
		}

		public string ConvertDestination() => Utils.UpperCaseStr(Destination);
		public string FancyDestination() => $"[{Svc.Data.GetExcelSheet<SubmarineExplorationPretty>(Dalamud.ClientLanguage.Japanese).GetRow(this.RowId).Location}] " + Utils.UpperCaseStr(Destination);
}
