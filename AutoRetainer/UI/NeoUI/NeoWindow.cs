using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public class NeoWindow
{
		private ConfigFileSystemEntry[] Tabs = [new MainSettings(), new CharaOrder(), new MultiModeRetainers(), new MultiModeDeployables()];

		private ConfigFileSystem<ConfigFileSystemEntry> FileSystem;

		public NeoWindow()
		{
				FileSystem = new(Tabs);
		}

		public void Draw()
		{
				FileSystem.Draw();
		}
}
