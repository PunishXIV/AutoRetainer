using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class LogTab : NeoUIEntry
{
		public override string Path => "Advanced/Log";

		public override void Draw()
		{
				InternalLog.PrintImgui();
		}
}
