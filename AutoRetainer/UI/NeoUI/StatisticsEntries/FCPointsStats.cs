using AutoRetainer.UI.Experiments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.StatisticsEntries;
public class FCPointsStats : NeoUIEntry
{
		public override string Path => "Statistics/FC Points";

		public override void Draw()
		{
        S.FCData.Draw();
		}
}
