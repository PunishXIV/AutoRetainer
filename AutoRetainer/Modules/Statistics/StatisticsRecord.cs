using Newtonsoft.Json;
using System.ComponentModel;

namespace AutoRetainer.Modules.Statistics;

[Serializable]
public class StatisticsRecord
{
		internal bool IsHQ
		{
				get
				{
						return IsHQInt != 0;
				}
				set
				{
						IsHQInt = value ? 1 : 0;
				}
		}

		[JsonProperty("I")] public uint ItemId;
		[JsonProperty("H")][DefaultValue(0)] public int IsHQInt = 0;
		[JsonProperty("T")] public long Timestamp;
		[JsonProperty("A")][DefaultValue(1)] public uint Amount = 1;
		[JsonProperty("V")][DefaultValue(0)] public uint VentureID = 0;
}
