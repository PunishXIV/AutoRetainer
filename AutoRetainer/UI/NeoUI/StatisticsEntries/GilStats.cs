using AutoRetainer.UI.Experiments;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.StatisticsEntries;
public class GilStats : NeoUIEntry
{
		public override string Path => "Statistics/Owned Gil";

		public override void Draw()
		{
        S.GilDisplay.Draw();
		}
}
