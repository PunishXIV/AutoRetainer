using AutoRetainer.UI.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class DebugTab : NeoUIEntry
{
		public override string Path => "Advanced/Debug";

		public override void Draw()
		{
				ImGuiEx.EzTabBar("DebugBar",
												("Retainers (old)", Retainers.Draw, null, true),
												("Debug", Debug.Draw, null, true),
												("WIP", SuperSecret.Draw, null, true));
		}

		public override bool ShouldDisplay()
		{
				return C.Verbose;
		}
}
