using ECommons.Configuration;

namespace AutoRetainer.Modules.Statistics;

[Serializable]
[IgnoreDefaultValue]
public class StatisticsFile : IEzConfig
{
		public List<StatisticsRecord> Records = [];
		public string PlayerName = $"Unnamed{Guid.NewGuid()}";
		public string RetainerName = $"Unnamed{Guid.NewGuid()}";
}
