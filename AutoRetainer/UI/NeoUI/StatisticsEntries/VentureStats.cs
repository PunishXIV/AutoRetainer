using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.StatisticsEntries;
public class VentureStats : NeoUIEntry
{
		public override string Path => "Statistics/Ventures";

		public override void Draw()
		{
				StatisticsUI.DrawVentures();
		}
}
